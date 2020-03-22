using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Maimai.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Maimai.Objects
{
    public class Hold : MaimaiHitObject, IHasHold
    {
        public double EndTime { get; set; }
    }
}
