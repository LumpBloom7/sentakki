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
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Checks;

public class CheckTouchAreaAndHitWindowOverlap : ICheck
{
    public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Compose, "Slide body window overlaps with other windows.");

    public IEnumerable<IssueTemplate> PossibleTemplates { get; }

    private TouchAreaAndHitWindowOverlapTemplate touchAreaAndHitWindowOverlapTemplate;

    public CheckTouchAreaAndHitWindowOverlap()
    {
        PossibleTemplates = [touchAreaAndHitWindowOverlapTemplate = new TouchAreaAndHitWindowOverlapTemplate(this)];
    }

    public IEnumerable<Issue> Run(BeatmapVerifierContext context)
    {
        var hitObjects = context.CurrentDifficulty.Playable.HitObjects;
        double slidePerfectWindow = new SentakkiSlideHitWindows().WindowFor(Rulesets.Scoring.HitResult.Great);
        double slideMissWindow = new SentakkiSlideHitWindows().WindowFor(Rulesets.Scoring.HitResult.Miss);

        double tapPerfectWindow = new SentakkiTapHitWindows().WindowFor(Rulesets.Scoring.HitResult.Great);
        double tapMissWindow = new SentakkiTapHitWindows().WindowFor(Rulesets.Scoring.HitResult.Miss);

        double touchPerfectWindow = new SentakkiTouchHitWindows().WindowFor(Rulesets.Scoring.HitResult.Great);
        double touchMissWindow = new SentakkiTouchHitWindows().WindowFor(Rulesets.Scoring.HitResult.Miss);

        List<Period> perfectWindowPeriods = [];
        List<Period> nonPerfectWindowPeriods = [];

        foreach (var hitObject in hitObjects)
        {
            switch (hitObject)
            {
                case TouchHold th:
                    perfectWindowPeriods.Add(new Period(th, th.StartTime, th.StartTime, th.Position, 130));
                    break;

                case Touch t:
                    perfectWindowPeriods.Add(new Period(hitObject, t.StartTime - touchPerfectWindow, t.StartTime + touchPerfectWindow, t.Position, 130));
                    nonPerfectWindowPeriods.Add(new Period(hitObject, hitObject.StartTime + touchPerfectWindow, hitObject.StartTime + touchMissWindow, t.Position, 130));
                    break;

                case Tap:
                case Hold:
                {
                    int lane = ((IHasLane)hitObject).Lane;

                    Vector2 position = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, lane);

                    perfectWindowPeriods.Add(new Period(hitObject, hitObject.StartTime - tapPerfectWindow, hitObject.StartTime + tapPerfectWindow, position, 100));
                    nonPerfectWindowPeriods.Add(new Period(hitObject, hitObject.StartTime - tapMissWindow, hitObject.StartTime - tapPerfectWindow, position, 100));
                    nonPerfectWindowPeriods.Add(new Period(hitObject, hitObject.StartTime + tapPerfectWindow, hitObject.StartTime + tapMissWindow, position, 100));
                    break;
                }

                case Slide slide:
                    if (slide.TapType is not Slide.TapTypeEnum.None)
                    {
                        Vector2 position = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, slide.Lane);

                        perfectWindowPeriods.Add(new Period(hitObject, hitObject.StartTime - tapPerfectWindow, hitObject.StartTime + tapPerfectWindow, position, 100));
                        nonPerfectWindowPeriods.Add(new Period(hitObject, hitObject.StartTime - tapMissWindow, hitObject.StartTime - tapPerfectWindow, position, 100));
                        nonPerfectWindowPeriods.Add(new Period(hitObject, hitObject.StartTime + tapPerfectWindow, hitObject.StartTime + tapMissWindow, position, 100));
                    }

                    foreach (var slideBodyInfo in slide.SlideInfoList)
                    {
                        int endLane = (slideBodyInfo.RelativeEndLane + slide.Lane).NormalizeLane();
                        double end_time = slide.StartTime + slideBodyInfo.EffectiveWaitDuration + slideBodyInfo.EffectiveMovementDuration;

                        Vector2 endPosition = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, endLane);

                        // We use the distance travelled per unit of time to determine how much leniency is present.
                        double dt_dx = slideBodyInfo.EffectiveMovementDuration / slideBodyInfo.SlideLength;

                        // This is the actual window to ensure that the player still gets the perfect when following the star perfectly
                        double visualPerfectWindow = DrawableSlideCheckpointNode.DETECTION_RADIUS / dt_dx;

                        double perfectWindowStart = end_time - Math.Max(visualPerfectWindow, slidePerfectWindow);
                        double perfectWindowEnd = end_time + slidePerfectWindow;

                        perfectWindowPeriods.Add(
                            new Period(slide, perfectWindowStart, perfectWindowEnd, endPosition, DrawableSlideCheckpointNode.DETECTION_RADIUS)
                        );

                        double earlyWindowStart = end_time - slideMissWindow;

                        if (earlyWindowStart < perfectWindowStart)
                        {
                            nonPerfectWindowPeriods.Add(
                                new Period(slide, earlyWindowStart, perfectWindowStart, endPosition, DrawableSlideCheckpointNode.DETECTION_RADIUS)
                            );
                        }

                        double lateWindowEnd = end_time + slideMissWindow;

                        nonPerfectWindowPeriods.Add(
                            new Period(slide, perfectWindowEnd, lateWindowEnd, endPosition, DrawableSlideCheckpointNode.DETECTION_RADIUS)
                        );
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
            yield return touchAreaAndHitWindowOverlapTemplate.Create(group);
    }

    public class TouchAreaAndHitWindowOverlapTemplate(ICheck check)
        : IssueTemplate(check, IssueType.Warning, $"Touch area of notes are overlapping, but the perfect window of one overlaps with the non-perfect window of another.")
    {
        public Issue Create(IEnumerable<HitObject> hitObjects) => new Issue(hitObjects, this);
    }

    private record Period(HitObject hitObject, double Start, double End, Vector2 position, float radius)
    {
        public bool IsOverlapped(Period other)
        {
            float distanceSquared = Vector2.DistanceSquared(position, other.position);
            float mutualExclusionDistanceSquared = MathF.Pow(radius + other.radius, 2);

            return distanceSquared < mutualExclusionDistanceSquared && Start.CompareTo(other.End) < 0 && other.Start.CompareTo(End) < 0;
        }
    }
}
