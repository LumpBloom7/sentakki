using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Bindables;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public abstract class SentakkiLanedHitObject : SentakkiHitObject
    {
        protected virtual bool NeedBreakSample => true;

        private List<HitSampleInfo> originalSamples { get; set; } = new List<HitSampleInfo>();

        public new IList<HitSampleInfo> Samples
        {
            get => originalSamples;
            set
            {
                originalSamples.Clear();
                originalSamples.AddRange(value);
            }
        }

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

            if (Break)
                for (int i = 0; i < 4; ++i)
                    AddNested(new ScorePaddingObject() { StartTime = this.GetEndTime() });
        }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            // Add break sample
            var sampleList = originalSamples.ToList();
            if (Break && NeedBreakSample) sampleList.Add(new SentakkiHitSampleInfo("Break"));
            base.Samples = sampleList;
        }
    }
}
