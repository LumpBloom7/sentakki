using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
using System;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Slide : SentakkiHitObject, IHasDuration
    {
        public override Color4 NoteColor => HasTwin ? Color4.Gold : Color4.Aqua;
        public static readonly float SLIDE_CHEVRON_DISTANCE = 25;
        public SentakkiSlidePath SlidePath;

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
            double nodeInterval = 1.0 / chevrons * 5; // Node every 5 chevrons.

            for (double progress = nodeInterval; progress < 1; progress += nodeInterval)
            {
                if (progress + nodeInterval >= 1)
                    AddNested(new SlideTailNode { StartTime = EndTime, Lane = Lane + SlidePath.EndLane });
                else
                    AddNested(new SlideNode { Progress = (float)progress });
            }
            AddNested(new Tap { Lane = Lane, StartTime = StartTime });
        }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
        public override Judgement CreateJudgement() => new IgnoreJudgement();

        public class SlideNode : SentakkiHitObject
        {
            public virtual float Progress { get; set; }
            protected override HitWindows CreateHitWindows() => HitWindows.Empty;
            public override Judgement CreateJudgement() => new IgnoreJudgement();
        }
        public class SlideTailNode : SlideNode
        {
            public override float Progress { get; set; } = 1;
            protected override HitWindows CreateHitWindows() => new SentakkiHitWindows();
            public override Judgement CreateJudgement() => new SentakkiJudgement();
        }
    }
}
