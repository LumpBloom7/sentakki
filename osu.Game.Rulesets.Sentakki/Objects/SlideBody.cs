using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Rulesets.Sentakki.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects;

public class SlideBody : SentakkiLanedHitObject, IHasDuration
{
    public override Color4 DefaultNoteColour => Color4.Aqua;

    protected override int BaseScoreWeighting => 3;

    public double EndTime
    {
        get => StartTime + Duration;
        set => Duration = value - StartTime;
    }

    public double Duration
    {
        get => SlideBodyInfo.Duration;
        set => SlideBodyInfo.Duration = value;
    }

    public SlideBodyInfo SlideBodyInfo { get; private set; }

    public SlideBody(SlideBodyInfo slideBodyInfo)
    {
        SlideBodyInfo = slideBodyInfo;
        Break = slideBodyInfo.Break;
        Ex = slideBodyInfo.Ex;
    }

    protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
    {
        createSlideCheckpoints();

        if (NestedHitObjects.Count != 0)
            NestedHitObjects.First().Samples.Add(new SentakkiHitSampleInfo("slide", CreateHitSampleInfo().Volume));

        base.CreateNestedHitObjects(cancellationToken);
    }

    private void createSlideCheckpoints()
    {
        float progress = 0;
        float totalDistance = (float)SlideBodyInfo.SlideLength;

        double shootTime = StartTime + SlideBodyInfo.WaitDuration;

        for (int i = 0; i < SlideBodyInfo.Segments.Count; ++i)
        {
            var segment = SlideBodyInfo.Segments[i];
            var segmentPath = SlideBodyInfo.SegmentPaths[i];

            bool isFanSegment = segment.Shape is PathShapes.Fan && i == SlideBodyInfo.Segments.Count - 1;

            float segmentDistance = (float)segmentPath.CalculatedDistance;
            float segmentRatio = segmentDistance / totalDistance;

            int numberOfCheckpoints = isFanSegment ? 5 : (int)Math.Floor(segmentDistance / 130);
            float progressDelta = 1f / numberOfCheckpoints;

            int nodesToPass = isFanSegment ? 2 : 1;

            for (int j = 0; j < numberOfCheckpoints; ++j)
            {
                progress += progressDelta * segmentRatio;

                List<Vector2> nodePositions = [SlideBodyInfo.PositionAt(progress)];

                if (isFanSegment)
                {
                    nodePositions.Add(SlideBodyInfo.PositionAt(progress, -1));
                    nodePositions.Add(SlideBodyInfo.PositionAt(progress, 1));
                }

                AddNested(new SlideCheckpoint
                {
                    Progress = progress,
                    StartTime = shootTime + progress * SlideBodyInfo.EffectiveMovementDuration,
                    NodePositions = nodePositions,
                    NodesToPass = nodesToPass,
                });
            }
        }
    }

    protected override HitWindows CreateHitWindows() => new SentakkiSlideHitWindows();
    public override Judgement CreateJudgement() => new SentakkiJudgement();
}
