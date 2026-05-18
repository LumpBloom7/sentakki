using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit.Checks;

public class CheckConcurrentLaneHitObjects : ICheck
{
    public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Compose, "Concurrent HitObjects");

    public IEnumerable<IssueTemplate> PossibleTemplates { get; }

    private IssueTemplateConcurrentSameLane issueTemplateConcurrentSameLane;
    private IssueTemplateAlmostConcurrentSameLane issueTemplateAlmostConcurrentSameLane;

    public CheckConcurrentLaneHitObjects()
    {
        PossibleTemplates = [
            issueTemplateConcurrentSameLane = new(this),
            issueTemplateAlmostConcurrentSameLane = new(this)
        ];
    }

    public virtual IEnumerable<Issue> Run(BeatmapVerifierContext context)
    {
        var lanes = context.CurrentDifficulty.Playable.HitObjects.OfType<SentakkiLanedHitObject>().GroupBy(h => h.Lane);

        // Check for true concurrency
        foreach (var lane in lanes)
        {
            HitObject? lastHitObject = lane.First();
            List<HitObject> hitObjects = [];

            foreach (var hitobject in lane)
            {
                // If the slide doesn't have a tap, then the tap can't ever be concurrent with other lane notes
                if (hitobject is Slide s && s.TapType is Slide.TapTypeEnum.None)
                    continue;

                if (lastHitObject is null)
                {
                    lastHitObject = hitobject;
                    hitObjects.Add(hitobject);
                    continue;
                }

                if (AreConcurrent(lastHitObject, hitobject))
                {
                    hitObjects.Add(hitobject);
                    if (laneEndTimes(lastHitObject) < laneEndTimes(hitobject))
                        lastHitObject = hitobject;

                    continue;
                }

                Issue? issue = emitConcurrentIssue(hitObjects);
                if (issue is not null)
                    yield return issue;

                lastHitObject = hitobject;
                hitObjects = [hitobject];
            }

            {
                var issue = emitConcurrentIssue(hitObjects);
                if (issue is not null)
                    yield return issue;
            }
        }

        // Check for near concurrency
        foreach (var lane in lanes)
        {
            HitObject? lastHitObject = lane.First();
            bool isTrueConcurrent = true;
            List<HitObject> hitObjects = [];

            foreach (var hitobject in lane)
            {
                // If the slide doesn't have a tap, then the tap can't ever be concurrent with other lane notes
                if (hitobject is Slide s && s.TapType is Slide.TapTypeEnum.None)
                    continue;

                if (lastHitObject is null)
                {
                    lastHitObject = hitobject;
                    hitObjects.Add(hitobject);
                    continue;
                }

                if (AreAlmostConcurrent(lastHitObject, hitobject))
                {
                    isTrueConcurrent = AreConcurrent(lastHitObject, hitobject) && isTrueConcurrent;
                    hitObjects.Add(hitobject);
                    if (laneEndTimes(lastHitObject) < laneEndTimes(hitobject))
                        lastHitObject = hitobject;

                    continue;
                }

                if (hitObjects.Count > 1 && !isTrueConcurrent)
                    yield return issueTemplateAlmostConcurrentSameLane.Create(hitObjects);

                lastHitObject = hitobject;
                isTrueConcurrent = true;
                hitObjects = [hitobject];
            }

            if (hitObjects.Count > 1 && !isTrueConcurrent)
                yield return issueTemplateAlmostConcurrentSameLane.Create(hitObjects);
        }
    }

    private Issue? emitConcurrentIssue(List<HitObject> hitObjects)
    {
        // We want to count the number of gameplay objects, not the decomposed variants in the editor
        int numObjects = hitObjects.Count(h => h is not Slide);

        // All concurrent slides in the same lane are merged
        if (hitObjects.Any(h => h is Slide))
            numObjects += 1;

        return numObjects > 1 ? issueTemplateConcurrentSameLane.Create(hitObjects) : null;
    }

    private static double laneEndTimes(HitObject hitObject) => hitObject is Slide s ? s.StartTime : hitObject.GetEndTime();

    // We guarantee that the objects are either treated as concurrent or unsnapped when near the same beat divisor.
    private const double ms_leniency = CheckUnsnappedObjects.UNSNAP_MS_THRESHOLD;
    private const double almost_concurrent_threshold = 10.0;

    protected static bool AreConcurrent(HitObject hitobject, HitObject nextHitobject) => nextHitobject.StartTime <= laneEndTimes(hitobject) + ms_leniency;

    protected static bool AreAlmostConcurrent(HitObject hitobject, HitObject nextHitobject) =>
        Math.Abs(nextHitobject.StartTime - laneEndTimes(hitobject)) < almost_concurrent_threshold;

    public class IssueTemplateConcurrentSameLane(ICheck check)
        : IssueTemplate(check, IssueType.Problem, "HitObjects are concurrent in the same lane.")
    {
        public Issue Create(IEnumerable<HitObject> hitObjects) => new Issue(hitObjects, this);
    }

    public class IssueTemplateAlmostConcurrentSameLane(ICheck check)
            : IssueTemplate(check, IssueType.Problem, "HitObjects are almost concurrent in the same lane.")
    {
        public Issue Create(IEnumerable<HitObject> hitObjects) => new Issue(hitObjects, this);
    }
}
