using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit.Checks;

// Yes it would be nice to inherit from CheckConcurrentObjects
// But we use slightly different issue templates.
public class CheckSentakkiConcurrentObjects : ICheck
{
    private SentakkiIssueTemplateConcurrentSame concurrentSameTemplate;
    private SentakkiIssueTemplateConcurrentDifferent concurrentDifferentTemplate;

    public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Compose, "Concurrent hitobjects");

    public IEnumerable<IssueTemplate> PossibleTemplates { get; }

    public CheckSentakkiConcurrentObjects()
    {
        concurrentSameTemplate = new SentakkiIssueTemplateConcurrentSame(this);
        concurrentDifferentTemplate = new SentakkiIssueTemplateConcurrentDifferent(this);
        PossibleTemplates = [concurrentSameTemplate, concurrentDifferentTemplate];
    }

    // We guarantee that the objects are either treated as concurrent or unsnapped when near the same beat divisor.
    private const double ms_leniency = CheckUnsnappedObjects.UNSNAP_MS_THRESHOLD;

    private bool areConcurrent(HitObject hitobject, HitObject nextHitobject) => nextHitobject.StartTime <= hitobject.GetEndTime() + ms_leniency;

    public IEnumerable<Issue> Run(BeatmapVerifierContext context) => checkLaneNotes(context).Concat(checkTouchNotes(context));

    private IEnumerable<Issue> checkTouchNotes(BeatmapVerifierContext context)
    {
        var hitObjects = context.Beatmap.HitObjects.Where(h => h is Touch or TouchHold).ToList();

        for (int i = 0; i < hitObjects.Count - 1; ++i)
        {
            var hitobject = hitObjects[i];

            for (int j = i + 1; j < hitObjects.Count; ++j)
            {
                var nextHitobject = hitObjects[j];

                // We only care if both objects simulatenously occupy the same position
                // TODO: This should also take into account closeness
                if (hitobject.GetPosition() != nextHitobject.GetPosition())
                    continue;

                // Two hitobjects cannot be concurrent without also being concurrent with all objects in between.
                // So if the next object is not concurrent, then we know no future objects will be either.
                if (!areConcurrent(hitobject, nextHitobject))
                    break;

                if (hitobject.GetType() == nextHitobject.GetType())
                    yield return new SentakkiIssueTemplateConcurrentSame(this).Create(hitobject, nextHitobject);
                else
                    yield return new SentakkiIssueTemplateConcurrentDifferent(this).Create(hitobject, nextHitobject);
            }
        }
    }

    private IEnumerable<Issue> checkLaneNotes(BeatmapVerifierContext context)
    {
        var hitObjects = context.Beatmap.HitObjects.Where(isLanedObject).Cast<SentakkiLanedHitObject>().ToList();

        for (int i = 0; i < hitObjects.Count - 1; ++i)
        {
            var hitobject = hitObjects[i];

            for (int j = i + 1; j < hitObjects.Count; ++j)
            {
                var nextHitobject = hitObjects[j];

                // We only care if both objects simulatenously occupy the same lane
                if (hitobject.Lane != nextHitobject.Lane)
                    continue;

                // Two hitobjects cannot be concurrent without also being concurrent with all objects in between.
                // So if the next object is not concurrent, then we know no future objects will be either.
                if (!areConcurrent(hitobject, nextHitobject))
                    break;

                if (hitobject.GetType() == nextHitobject.GetType())
                    yield return new SentakkiIssueTemplateConcurrentSame(this).Create(hitobject, nextHitobject);
                else
                    yield return new SentakkiIssueTemplateConcurrentDifferent(this).Create(hitobject, nextHitobject);
            }
        }

        yield break;
    }

    private bool isLanedObject(HitObject hitObject) => hitObject switch
    {
        Tap => true,
        Hold => true,
        // Slides can't inherently overlap with anything, however their slide taps can
        Slide s when s.TapType is not Slide.TapTypeEnum.None => true,
        _ => false,
    };

    private abstract class SentakkiIssueTemplateConcurrent(ICheck check, string unformattedMessage)
        : IssueTemplate(check, IssueType.Problem, unformattedMessage)
    {
        public Issue Create(HitObject hitobject, HitObject nextHitobject)
        {
            var hitobjects = new List<HitObject> { hitobject, nextHitobject };

            // In order to highlight the slides properly when clicking on the issue
            // We pass the parenting slide, and not the slide taps.
            // However, we still want to refer to it properly.
            string name1 = hitobject is Slide ? nameof(SlideTap) : hitobject.GetType().Name;
            string name2 = nextHitobject is Slide ? nameof(SlideTap) : hitobject.GetType().Name;

            return new Issue(hitobjects, this, name1, name2)
            {
                Time = nextHitobject.StartTime
            };
        }
    }

    private class SentakkiIssueTemplateConcurrentSame(ICheck check) : SentakkiIssueTemplateConcurrent(check, "{0}s are concurrent here.");

    private class SentakkiIssueTemplateConcurrentDifferent(ICheck check) : SentakkiIssueTemplateConcurrent(check, "{0} and {1} are concurrent here.");
}
