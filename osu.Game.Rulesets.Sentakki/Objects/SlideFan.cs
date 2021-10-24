using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Scoring;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class SlideFan : SentakkiLanedHitObject, IHasDuration
    {
        public override Color4 DefaultNoteColour => Color4.Aqua;

        protected override bool NeedBreakSample => false;

        [JsonIgnore]
        public double ShootDelayAbsolute { get; private set; }

        public double ShootDelay { get; set; } = 1;

        public double Duration { get; set; }
        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public List<IList<HitSampleInfo>> NodeSamples = new List<IList<HitSampleInfo>>();

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            AddNested(new SlideTap
            {
                LaneBindable = { BindTarget = LaneBindable },
                StartTime = StartTime,
                Samples = NodeSamples.Any() ? NodeSamples.First() : new List<HitSampleInfo>(),
                Break = Break
            });

            // Add body nodes (should be two major sets)
        }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            double delay = controlPointInfo.TimingPointAt(StartTime).BeatLength * ShootDelay / 2;
            if (delay < Duration - 50)
                ShootDelayAbsolute = delay;
        }

        protected override HitWindows CreateHitWindows() => new SentakkiSlideHitWindows();
    }
}
