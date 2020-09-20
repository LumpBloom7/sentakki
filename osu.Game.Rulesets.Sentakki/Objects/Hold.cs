using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Scoring;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Hold : SentakkiLanedHitObject, IHasDuration
    {
        public override bool IsBreak { get; set; }

        private List<IList<HitSampleInfo>> nodeSamples = new List<IList<HitSampleInfo>>();

        public List<IList<HitSampleInfo>> NodeSamples
        {
            get => nodeSamples;
            set
            {
                Samples = value.Last();
                Head.Samples = value.First();
            }
        }

        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public double Duration { get; set; }

        public override double StartTime
        {
            get => base.StartTime;
            set
            {
                base.StartTime = value;
                Head.StartTime = value;
            }
        }

        public override int Lane
        {
            get => base.Lane;
            set
            {
                base.Lane = value;
                Head.Lane = value;
            }
        }

        public readonly HoldHead Head = new HoldHead();

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();

            AddNested(Head);
        }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        public class HoldHead : SentakkiLanedHitObject
        {
            public override Judgement CreateJudgement() => new IgnoreJudgement();
            protected override HitWindows CreateHitWindows() => new SentakkiHitWindows();
        }
    }
}
