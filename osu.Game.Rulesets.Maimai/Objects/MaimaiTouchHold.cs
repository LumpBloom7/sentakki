using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Maimai.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Maimai.Objects.Drawables
{
    public class MaimaiTouchHold : MaimaiHitObject, IHasEndTime
    {
        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public double Duration { get; set; }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
