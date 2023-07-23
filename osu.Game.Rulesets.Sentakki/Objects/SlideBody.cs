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
using osu.Game.Rulesets.Sentakki.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class SlideBody : SentakkiLanedHitObject, IHasDuration
    {
        public override Color4 DefaultNoteColour => Color4.Aqua;

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
        }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            CreateSlideCheckpoints();

            if (NestedHitObjects.Any())
                NestedHitObjects[0].Samples.Add(new SentakkiHitSampleInfo("slide", CreateHitSampleInfo().Volume));
        }

        protected void CreateSlideCheckpoints()
        {
            double totalDistance = SlideBodyInfo.SlidePath.TotalDistance;
            double runningDistance = 0;

            foreach (var segment in SlideBodyInfo.SlidePath.SlideSegments)
            {
                double distance = segment.Distance;
                int nodeCount = (int)Math.Floor(distance / 130);

                double nodeDelta = distance / nodeCount;

                for (int i = 0; i < nodeCount; i++)
                {
                    runningDistance += nodeDelta;
                    double progress = runningDistance / totalDistance;

                    SlideCheckpoint checkpoint = new SlideCheckpoint
                    {
                        Progress = (float)progress,
                        StartTime = StartTime + ShootDelay + ((Duration - ShootDelay) * progress),
                        NodePositions = new List<Vector2> { SlideBodyInfo.SlidePath.PositionAt(progress) }
                    };

                    AddNested(checkpoint);
                }
            }

            CreateSlideFanCheckpoints();
        }

        protected void CreateSlideFanCheckpoints()
        {
            if (!SlideBodyInfo.SlidePath.EndsWithSlideFan)
                return;

            // Add body nodes (should be two major sets)
            Vector2 originpoint = SlideBodyInfo.SlidePath.FanOrigin;

            for (int i = 1; i < 5; ++i)
            {
                float progress = SlideBodyInfo.SlidePath.FanStartProgress + (0.25f * i * (1 - SlideBodyInfo.SlidePath.FanStartProgress));
                SlideCheckpoint checkpoint = new SlideCheckpoint
                {
                    Progress = progress,
                    StartTime = StartTime + ShootDelay + ((Duration - ShootDelay) * progress),
                    NodesToPass = 2,
                };

                for (int j = -1; j < 2; ++j)
                {
                    Vector2 dest = SlideBodyInfo.SlidePath.PositionAt(1, j);
                    checkpoint.NodePositions.Add(Vector2.Lerp(originpoint, dest, 0.25f * i));
                }

                AddNested(checkpoint);
            }
        }

        [JsonIgnore]
        public double ShootDelay { get; private set; }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            double delay = controlPointInfo.TimingPointAt(StartTime).BeatLength * SlideBodyInfo.ShootDelay;
            if (delay < Duration - 50)
                ShootDelay = delay;
        }

        protected override HitWindows CreateHitWindows() => new SentakkiSlideHitWindows();
        public override Judgement CreateJudgement() => new SentakkiJudgement();
    }
}
