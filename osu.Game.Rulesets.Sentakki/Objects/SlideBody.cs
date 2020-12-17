using System;
using System.Threading;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
using osu.Game.Rulesets.Sentakki.Scoring;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class SlideBody : SentakkiLanedHitObject, IHasDuration
    {
        protected override Color4 DefaultNoteColour => Color4.Aqua;
        public static readonly float SLIDE_CHEVRON_DISTANCE = 30f;

        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public double Duration
        {
            get => SlideInfo.Duration;
            set => SlideInfo.Duration = value;
        }

        public SentakkiSlideInfo SlideInfo { get; set; }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            var distance = SlideInfo.SlidePath.Path.Distance;
            int chevrons = (int)Math.Round(distance / SLIDE_CHEVRON_DISTANCE);
            double chevronInterval = 1.0 / chevrons;

            for (int i = 4; i < chevrons - 1; i += 3)
            {
                var progress = i * chevronInterval;
                SlideNode node;
                AddNested(node = new SlideNode
                {
                    StartTime = StartTime + ShootDelay + ((Duration - ShootDelay) * progress),
                    Progress = (float)progress
                });

                // Add the slide sample to first node
                if (i == 4)
                    node.Samples.Add(new SentakkiHitSampleInfo("slide"));
            }

            AddNested(new SlideNode
            {
                StartTime = EndTime,
                Progress = 1
            });
        }

        [JsonIgnore]
        public double ShootDelay { get; private set; }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            double delay = controlPointInfo.TimingPointAt(StartTime).BeatLength * SlideInfo.ShootDelay / 2;
            if (delay < Duration - 50)
                ShootDelay = delay;
        }

        protected override HitWindows CreateHitWindows() => new SentakkiSlideHitWindows();
        public override Judgement CreateJudgement() => new SentakkiJudgement();

        public class SlideNode : SentakkiHitObject
        {
            public virtual float Progress { get; set; }

            protected override HitWindows CreateHitWindows() => HitWindows.Empty;
            public override Judgement CreateJudgement() => new IgnoreJudgement();
        }
    }
}
