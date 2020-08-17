using osu.Framework.Bindables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
using System;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Slide : SentakkiLanedHitObject, IHasDuration
    {
        public override Color4 NoteColor => IsBreak ? Color4.OrangeRed : HasTwin ? Color4.Gold : Color4.Aqua;
        public static readonly float SLIDE_CHEVRON_DISTANCE = 25;
        public SentakkiSlidePath SlidePath;

        // The delay (in beats) before the animation star starts moving along the path
        private BindableInt slideShootDelay = new BindableInt(1);

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
                Lane = Lane + SlidePath.EndLane,
                Progress = 1
            });

            AddNested(new Tap { Lane = Lane, StartTime = StartTime, Samples = Samples, IsBreak = IsBreak });
        }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
        public override Judgement CreateJudgement() => new IgnoreJudgement();

        public class SlideNode : SentakkiLanedHitObject
        {
            public virtual float Progress { get; set; }

            public bool IsTailNote => Progress == 1;
            protected override HitWindows CreateHitWindows() => IsTailNote ? new SentakkiSlideHitWindows() : HitWindows.Empty;
            public override Judgement CreateJudgement() => IsTailNote ? new SentakkiJudgement() : (Judgement)new IgnoreJudgement();
        }
    }
}
