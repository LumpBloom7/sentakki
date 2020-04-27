using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
using osu.Game.Rulesets.Sentakki.Scoring;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Break : Tap
    {
        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();
            for (int i = 0; i < 4; ++i)
                AddNested(new Child());
        }
        public override double StartTime
        {
            get => base.StartTime;
            set
            {
                base.StartTime = value;
            }
        }

        public override Judgement CreateJudgement() => new SentakkiJudgement();
        protected override HitWindows CreateHitWindows() => new SentakkiHitWindows();

        public class Child : SentakkiHitObject
        {
            protected override HitWindows CreateHitWindows() => new HitWindows.EmptyHitWindows();
        }
    }
}
