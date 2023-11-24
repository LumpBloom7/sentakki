using System;
using System.Collections.Generic;
using System.Threading;
using osu.Framework.Bindables;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public abstract class SentakkiLanedHitObject : SentakkiHitObject
    {
        protected virtual bool NeedBreakSample => true;

        public override int ScoreWeighting => Break ? 5 : 1;

        public readonly BindableBool BreakBindable = new BindableBool();

        public bool Break
        {
            get => BreakBindable.Value;
            set => BreakBindable.Value = value;
        }

        public readonly BindableInt LaneBindable = new BindableInt();

        public int Lane
        {
            get => LaneBindable.Value;
            set => LaneBindable.Value = value;
        }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            for (int i = 1; i < ScoreWeighting; ++i)
                AddNested(new ScorePaddingObject { StartTime = this.GetEndTime() });

            if (Break)
                AddNested(new ScoreBonusObject
                {
                    StartTime = this.GetEndTime(),
                    HitWindows = HitWindows
                });
        }

        public override IList<HitSampleInfo> AuxiliarySamples => CreateBreakSample();

        public HitSampleInfo[] CreateBreakSample()
        {
            if (!NeedBreakSample || !Break)
                return Array.Empty<HitSampleInfo>();

            return new[]
            {
                new SentakkiHitSampleInfo("Break", CreateHitSampleInfo().Volume)
            };
        }
    }
}
