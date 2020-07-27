using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Scoring;
using System.Collections.Generic;
using System.Linq;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Hold : SentakkiHitObject, IHasDuration
    {
        private bool isBreak = false;
        public override bool IsBreak
        {
            get => isBreak;
            set
            {
                isBreak = value;
                Head.IsBreak = value;
            }
        }
        private List<IList<HitSampleInfo>> nodeSamples = new List<IList<HitSampleInfo>>();

        public List<IList<HitSampleInfo>> NodeSamples
        {
            get => nodeSamples;
            set
            {
                Tail.Samples = value.Last();
                Head.Samples = value.First();
            }
        }

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

        public override int Lane
        {
            get => base.Lane;
            set
            {
                base.Lane = value;
                Head.Lane = value;
                Tail.Lane = value;
            }
        }

        public readonly Tap Head = new Tap();

        public readonly HoldTail Tail = new HoldTail();

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();

            AddNested(Head);
            AddNested(Tail);
        }

        public override Judgement CreateJudgement() => new IgnoreJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        public class HoldTail : SentakkiHitObject
        {
            protected override HitWindows CreateHitWindows() => new SentakkiHoldHitWindows();
        }
    }
}
