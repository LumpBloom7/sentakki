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

        public virtual int ScoreWeighting => Break ? 5 : 1;

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
        }

        public override IList<HitSampleInfo> AuxiliarySamples => new HitSampleInfo[] { new BreakSample(CreateHitSampleInfo()) };

        public HitSampleInfo[] CreateBreakSample()
        {
            if (!NeedBreakSample || !Break)
                return Array.Empty<HitSampleInfo>();

            return new[]
            {
                new BreakSample( CreateHitSampleInfo())
            };
        }

        public class BreakSample : HitSampleInfo
        {
            public override IEnumerable<string> LookupNames
            {
                get
                {
                    foreach (string name in base.LookupNames)
                        yield return name;

                    foreach (string name in base.LookupNames)
                        yield return name.Replace("-max", string.Empty);
                }
            }

            public BreakSample(HitSampleInfo sampleInfo)
                : base("spinnerbonus-max", sampleInfo.Bank, sampleInfo.Suffix, sampleInfo.Volume)

            {
            }
        }
    }
}
