using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
using osu.Game.Rulesets.Sentakki.Scoring;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Hold : SentakkiLanedHitObject, IHasDuration
    {
        protected override bool PlaysBreakSample => false;

        private IList<IList<HitSampleInfo>>? nodeSamples = null;

        public IList<IList<HitSampleInfo>>? NodeSamples
        {
            get => nodeSamples;
            set
            {
                Samples = value?.Last();
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
            // Editor context doesn't provide NodeSamples, we must make sane defaults from Samples
            // We explicitly create a new list with the same elements instead of directly using the Samples
            // This is because assigning Samples with itself, will clear the list before adding the now empty list
            NodeSamples ??=
            [
                [.. Samples],
                [.. Samples],
            ];

            AddNested(new HoldHead
            {
                Break = Break,
                StartTime = StartTime,
                Lane = Lane,
                Samples = nodeSamples?.First() ?? [],
                Ex = Ex
            });

            base.CreateNestedHitObjects(cancellationToken);
        }

        protected override HitWindows CreateHitWindows() => new SentakkiHoldReleaseWindows();

        public class HoldHead : SentakkiLanedHitObject
        {
            public override Judgement CreateJudgement() => new SentakkiJudgement();
            protected override HitWindows CreateHitWindows() => new SentakkiTapHitWindows();
        }
    }
}
