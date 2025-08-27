using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit.Checks;

public class CheckSlideTapOmission : ICheck
{
    private readonly SlideTapOmittedIssueTemplate omittedTapTemplate;
    private readonly MergeSlideWithOmittedTapIssueTemplate mergeSlideTemplate;
    private readonly ReplaceTapWithSlideTapIssueTemplate replaceTapTemplate;

    public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.HitObjects, "Incorrectly omitted slide tap");
    public IEnumerable<IssueTemplate> PossibleTemplates { get; }

    public CheckSlideTapOmission()
    {
        omittedTapTemplate = new SlideTapOmittedIssueTemplate(this);
        mergeSlideTemplate = new MergeSlideWithOmittedTapIssueTemplate(this);
        replaceTapTemplate = new ReplaceTapWithSlideTapIssueTemplate(this);

        PossibleTemplates = [omittedTapTemplate, mergeSlideTemplate, replaceTapTemplate];
    }

    // We guarantee that the objects are either treated as concurrent or unsnapped when near the same beat divisor.
    private const double ms_leniency = CheckUnsnappedObjects.UNSNAP_MS_THRESHOLD;

    private bool areConcurrent(HitObject hitobject, HitObject nextHitobject) => nextHitobject.StartTime <= hitobject.GetEndTime() + ms_leniency;

    public IEnumerable<Issue> Run(BeatmapVerifierContext context)
    {
        var hitObjects = context.CurrentDifficulty.Playable.HitObjects.Where(h => h is Slide or Tap).Cast<SentakkiLanedHitObject>().ToList();

        for (int i = 0; i < hitObjects.Count; ++i)
        {
            var currentHitobject = hitObjects[i];

            if (currentHitobject is Slide s && s.TapType is Slide.TapTypeEnum.None)
                yield return omittedTapTemplate.Create(s);

            for (int j = i + 1; j < hitObjects.Count; ++j)
            {
                var nextHitObject = hitObjects[j];

                // We only care if both objects simultaneously occupy the same lane
                if (currentHitobject.Lane != nextHitObject.Lane)
                    continue;

                if (!areConcurrent(currentHitobject, nextHitObject))
                    break;

                if (currentHitobject is Slide s1 && nextHitObject is Slide s2)
                {
                    if (shouldBeMerged(s1, s2))
                        yield return mergeSlideTemplate.Create(currentHitobject, nextHitObject);
                }
                else if (concurrentWithSlideTap(currentHitobject, nextHitObject))
                {
                    yield return replaceTapTemplate.Create(currentHitobject, nextHitObject);
                }
            }
        }
    }

    private static bool shouldBeMerged(Slide first, Slide second)
    {
        if (first.TapType == second.TapType)
            return false;

        return first.TapType is Slide.TapTypeEnum.None || second.TapType is Slide.TapTypeEnum.None;
    }

    private static bool concurrentWithSlideTap(HitObject first, HitObject second)
    {
        if (first is Slide s1 && s1.TapType is Slide.TapTypeEnum.None && second is Tap)
            return true;

        return second is Slide s2 && s2.TapType is Slide.TapTypeEnum.None && first is Tap;
    }

    private class SlideTapOmittedIssueTemplate(ICheck check)
        : IssueTemplate(check, IssueType.Warning, $"{nameof(Slide)} omits initial tap here. This will slightly alter {nameof(SlideBody)} input behaviour.")
    {
        public Issue Create(Slide slide) => new Issue(slide, this);
    }

    private class MergeSlideWithOmittedTapIssueTemplate(ICheck check)
        : IssueTemplate(check, IssueType.Problem, $"{nameof(Slide)}s are concurrent here, but one omits the initial tap. Consider merging for better game feel.")
    {
        public Issue Create(HitObject first, HitObject second) => new Issue([first, second], this);
    }

    private class ReplaceTapWithSlideTapIssueTemplate(ICheck check)
        : IssueTemplate(check, IssueType.Problem,
            $"{nameof(Slide)} with omitted initial tap is concurrent with a {nameof(Tap)} here.")
    {
        public Issue Create(HitObject first, HitObject second) => new Issue([first, second], this);
    }
}
