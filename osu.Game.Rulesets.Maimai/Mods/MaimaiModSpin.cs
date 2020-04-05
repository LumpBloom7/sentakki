// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Maimai.Objects;
using osu.Game.Rulesets.Maimai.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Maimai.Mods
{
    public class MaimaiModSpin : Mod, IApplicableToDrawableRuleset<MaimaiHitObject>
    {
        public override string Name => "Spin";
        public override string Description => "Replicate the true washing machine experience.";
        public override string Acronym => "S";

        public override IconUsage? Icon => FontAwesome.Solid.RedoAlt;
        public override ModType Type => ModType.Fun;

        public override bool Ranked => false;

        public override double ScoreMultiplier => 1.00;

        public void ApplyToDrawableRuleset(DrawableRuleset<MaimaiHitObject> drawableRuleset)
        {
            (drawableRuleset.Playfield as MaimaiPlayfield).spinMod.Value = true;
        }
    }
}
