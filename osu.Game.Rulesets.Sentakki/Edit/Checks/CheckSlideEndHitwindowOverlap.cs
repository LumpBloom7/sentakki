using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Types;
using osu.Game.Rulesets.Sentakki.Scoring;

namespace osu.Game.Rulesets.Sentakki.Edit.Checks;

public class CheckSlideEndHitWindowOverlap : ICheck
{
    public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Compose, "Slide body window overlaps with other windows.");

    public IEnumerable<IssueTemplate> PossibleTemplates { get; }

    private SlideEndHitWindowOverlapTemplate slideEndHitWindowOverlapTemplate;

    public CheckSlideEndHitWindowOverlap()
    {
        PossibleTemplates = [slideEndHitWindowOverlapTemplate = new SlideEndHitWindowOverlapTemplate(this)];
    }

    public IEnumerable<Issue> Run(BeatmapVerifierContext context)
    {
        var hitObjects = context.CurrentDifficulty.Playable.HitObjects;
        double slidePerfectWindow = new SentakkiSlideHitWindows().WindowFor(Rulesets.Scoring.HitResult.Perfect);
        double slideMissWindow = new SentakkiSlideHitWindows().WindowFor(Rulesets.Scoring.HitResult.Miss);

        double tapPerfectWindow = new SentakkiTapHitWindows().WindowFor(Rulesets.Scoring.HitResult.Perfect);
        double tapMissWindow = new SentakkiTapHitWindows().WindowFor(Rulesets.Scoring.HitResult.Miss);

        List<Period> perfectWindowPeriods = [];
        List<Period> nonPerfectWindowPeriods = [];

        foreach (var hitObject in hitObjects)
        {
            switch (hitObject)
            {
                case Tap:
                case Hold:
                    int lane = ((IHasLane)hitObject).Lane;
                    nonPerfectWindowPeriods.Add(new Period(hitObject, lane, hitObject.StartTime - tapMissWindow, hitObject.StartTime - tapPerfectWindow));
                    nonPerfectWindowPeriods.Add(new Period(hitObject, lane, hitObject.StartTime + tapPerfectWindow, hitObject.StartTime + tapMissWindow));
                    break;

                case Slide slide:
                    if (slide.TapType is not Slide.TapTypeEnum.None)
                    {
                        nonPerfectWindowPeriods.Add(new Period(hitObject, slide.Lane, hitObject.StartTime - tapMissWindow, hitObject.StartTime - tapPerfectWindow));
                        nonPerfectWindowPeriods.Add(new Period(hitObject, slide.Lane, hitObject.StartTime + tapPerfectWindow, hitObject.StartTime + tapMissWindow));
                    }

                    foreach (var slideBodyInfo in slide.SlideInfoList)
                    {
                        int endLane = (slideBodyInfo.RelativeEndLane + slide.Lane).NormalizeLane();
                        double end_time = slide.StartTime + slideBodyInfo.EffectiveWaitDuration + slideBodyInfo.EffectiveMovementDuration;

                        // We use the distance travelled per unit of time to determine how much leniency is present.
                        double dt_dx = slideBodyInfo.EffectiveMovementDuration / slideBodyInfo.SlideLength;

                        // This is the actual window to ensure that the player still gets the perfect when following the star perfectly
                        double visualPerfectWindow = DrawableSlideCheckpointNode.DETECTION_RADIUS / dt_dx;

                        double perfectWindowStart = end_time - Math.Max(visualPerfectWindow, slidePerfectWindow);
                        double perfectWindowEnd = end_time + slidePerfectWindow;

                        perfectWindowPeriods.Add(new Period(slide, endLane, perfectWindowStart, perfectWindowEnd));

                        double earlyWindowStart = end_time - slideMissWindow;

                        if (earlyWindowStart < perfectWindowStart)
                            nonPerfectWindowPeriods.Add(new Period(slide, endLane, earlyWindowStart, perfectWindowStart));

                        double lateWindowEnd = end_time + slideMissWindow;
                        nonPerfectWindowPeriods.Add(new Period(slide, endLane, perfectWindowEnd, lateWindowEnd));
                    }
                    break;
            }
        }

        List<HashSet<HitObject>> overlappingGroups = [];

        foreach (var period in perfectWindowPeriods)
        {
            HashSet<HitObject> overlapping = [period.hitObject];

            foreach (var otherPeriod in nonPerfectWindowPeriods)
            {
                if (otherPeriod.End < period.Start)
                    continue;

                if (otherPeriod.Start > period.End)
                    break;

                if (otherPeriod.hitObject == period.hitObject)
                    continue;

                if (period.IsOverlapped(otherPeriod))
                    overlapping.Add(otherPeriod.hitObject);
            }

            if (overlapping.Count == 1)
                continue;

            if (overlappingGroups.Any(h => h.IsSupersetOf(overlapping)))
                continue;

            overlappingGroups.Add(overlapping);
        }

        foreach (var group in overlappingGroups)
            yield return slideEndHitWindowOverlapTemplate.Create(group);
    }

    public class SlideEndHitWindowOverlapTemplate(ICheck check)
        : IssueTemplate(check, IssueType.Warning, $"{nameof(Slide)} body perfect window overlaps with non-perfect window of another note.")
    {
        public Issue Create(IEnumerable<HitObject> hitObjects) => new Issue(hitObjects, this);
    }

    private record Period(HitObject hitObject, int lane, double Start, double End)
    {
        public bool IsOverlapped(Period other)
        {
            return lane == other.lane && Start.CompareTo(other.End) < 0 && other.Start.CompareTo(End) < 0;
        }
    }
}
