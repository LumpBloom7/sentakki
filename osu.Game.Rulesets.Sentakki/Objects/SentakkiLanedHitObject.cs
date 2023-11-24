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

        public readonly BindableBool BreakBindable = new BindableBool();

        protected virtual int ScoreWeightingMultiplier => 1;

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

            for (int j = 0; j < ScoreWeightingMultiplier; ++j)
            {
                if (j > 0)
                    AddNested(new ScorePaddingObject { StartTime = this.GetEndTime() }); //Add more weight

                if (Break)
                {
                    for (int i = 0; i < 4; ++i)
                        AddNested(new ScorePaddingObject { StartTime = this.GetEndTime() });

                    // Add bonus for players hitting within the critical window
                    AddNested(new ScoreBonusObject
                    {
                        StartTime = this.GetEndTime(),
                        HitWindows = HitWindows
                    });
                }
            }
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
