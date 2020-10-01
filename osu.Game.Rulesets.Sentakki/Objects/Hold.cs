using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
                nodeSamples = value;
            }
        }

        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public double Duration { get; set; }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            AddNested(new HoldHead
            {
                IsBreak = IsBreak,
                StartTime = StartTime,
                Lane = Lane,
                Samples = nodeSamples.First()
            });
        }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        public class HoldHead : SentakkiLanedHitObject
        {
            public override Judgement CreateJudgement() => new SentakkiJudgement();
            protected override HitWindows CreateHitWindows() => new SentakkiHitWindows();
        }
    }
}
