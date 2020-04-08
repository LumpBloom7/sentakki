using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Maimai.Objects
{
    public class Hold : MaimaiHitObject, IHasEndTime
    {
        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        private double duration;

        public double Duration
        {
            get => duration;
            set
            {
                duration = value;
                Tail.StartTime = EndTime;
            }
        }

        public override double StartTime
        {
            get => base.StartTime;
            set
            {
                base.StartTime = value;
                Head.StartTime = value;
                Tail.StartTime = EndTime;
            }
        }

        public override float Angle
        {
            get => base.Angle;
            set
            {
                base.Angle = value;
                Head.Angle = value;
                Tail.Angle = value;
            }
        }

        public readonly Tap Head = new Tap();

        public readonly HoldTail Tail = new HoldTail();

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();

            Head.Samples = Samples;

            AddNested(Head);
            AddNested(Tail);
        }

        public override Judgement CreateJudgement() => new IgnoreJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
