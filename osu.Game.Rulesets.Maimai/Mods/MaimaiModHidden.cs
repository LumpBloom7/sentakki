// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Maimai.Objects.Drawables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Maimai.Mods
{
    public class MaimaiModHidden : ModHidden
    {
        public override string Description => @"Play with fading notes.";
        public override double ScoreMultiplier => 1.06;

        public override void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var d in drawables.OfType<DrawableMaimaiHitObject>())
            {
                d.IsHidden = true;
            }
        }
    }
}
