using osu.Framework.Bindables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
using System;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class SlideBody : SentakkiHitObject, IHasDuration
    {
        // The delay (in beats) before the animation star starts moving along the path
        private readonly BindableInt slideShootDelay = new BindableInt(1);

        public int SlideShootDelay
        {
            get => slideShootDelay.Value;
            set => slideShootDelay.Value = value;
        }

        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public double Duration { get; set; }

        public SentakkiSlidePath SlidePath;

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();

            var distance = SlidePath.Path.Distance;
            int chevrons = (int)Math.Ceiling(distance / Slide.SLIDE_CHEVRON_DISTANCE);
            double chevronInterval = 1.0 / chevrons;

            for (int i = 5; i < chevrons - 2; i += 5)
            {
                var progress = i * chevronInterval;
                AddNested(new SlideNode
                {
                    Progress = (float)progress
                });
            }

            AddNested(new SlideNode
            {
                StartTime = EndTime,
                Progress = 1
            });
        }

        protected override HitWindows CreateHitWindows() => new SentakkiSlideHitWindows();
        public override Judgement CreateJudgement() => new SentakkiJudgement();

        public class SlideNode : SentakkiHitObject
        {
            public virtual float Progress { get; set; }

            public bool IsTailNote => Progress == 1;
            protected override HitWindows CreateHitWindows() => HitWindows.Empty;
            public override Judgement CreateJudgement() => new IgnoreJudgement();
        }
    }
}