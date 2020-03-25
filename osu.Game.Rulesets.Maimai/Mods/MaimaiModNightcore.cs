// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Maimai.Objects;
using osu.Game.Rulesets.Maimai.Replays;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Rulesets.Maimai.Mods
{
    public class MaimaiModNightcore : ModNightcore<MaimaiHitObject>
    {
        public override double ScoreMultiplier => 1.12;
    }
}
