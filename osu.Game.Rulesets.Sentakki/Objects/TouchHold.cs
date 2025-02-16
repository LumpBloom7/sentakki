﻿using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Scoring;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class TouchHold : SentakkiHitObject, IHasDuration
    {
        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public double Duration { get; set; }

        protected override HitWindows CreateHitWindows() => new SentakkiEmptyHitWindows();

        public override IList<HitSampleInfo> AuxiliarySamples => CreateHoldSample();

        public HitSampleInfo[] CreateHoldSample()
        {
            var referenceSample = Samples.FirstOrDefault();

            if (referenceSample == null)
                return [];

            return [referenceSample.With("spinnerspin")];
        }
    }
}
