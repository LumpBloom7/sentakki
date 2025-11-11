using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
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
        double progress = 0;
        double totalDistance = SlideBodyInfo.SlideLength;

        double shootTime = StartTime + SlideBodyInfo.HoldDuration;

        for (int i = 0; i < SlideBodyInfo.Segments.Count; ++i)
        {
            var segment = SlideBodyInfo.Segments[i];
            var segmentPath = SlideBodyInfo.SegmentPaths[i];

            bool isFanSegment = segment.Shape is PathShapes.Fan && i == SlideBodyInfo.Segments.Count - 1;

            double segmentDistance = segmentPath.CalculatedDistance;

            int numberOfCheckpoints = isFanSegment ? 5 : (int)Math.Floor(segmentDistance / 130);
            double progressDelta = (segmentDistance / totalDistance) / numberOfCheckpoints;

            int nodesToPass = isFanSegment ? 2 : 1;

            for (int j = 0; j < numberOfCheckpoints; ++j)
            {
                progress += progressDelta;

                List<Vector2> nodePositions = [SlideBodyInfo.PositionAt((float)progress)];

                if (isFanSegment)
                {
                    nodePositions.Add(SlideBodyInfo.PositionAt((float)progress, -1));
                    nodePositions.Add(SlideBodyInfo.PositionAt((float)progress, 1));
                }

                AddNested(new SlideCheckpoint
                {
                    Progress = (float)progress,
                    StartTime = shootTime + progress * SlideBodyInfo.MovementDuration,
                    NodePositions = nodePositions,
                    NodesToPass = nodesToPass,
                });
            }
        }
    }

    [JsonIgnore]
    public double ShootDelay { get; private set; }

    protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
    {
        base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

        ShootDelay = Math.Clamp(SlideBodyInfo.HoldDuration, 0, Duration);
    }

    protected override HitWindows CreateHitWindows() => new SentakkiSlideHitWindows();
    public override Judgement CreateJudgement() => new SentakkiJudgement();
}
