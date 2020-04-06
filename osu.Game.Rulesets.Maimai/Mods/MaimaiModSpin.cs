// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Maimai.Objects;
using osu.Game.Rulesets.Maimai.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Framework.Bindables;
using osu.Game.Configuration;

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

        [SettingSource("Revolution Duration", "The duration in seconds to complete a revolution")]
        public BindableNumber<int> revolutionDuration { get; } = new BindableNumber<int>
        {
            MinValue = 3,
            MaxValue = 10,
            Default = 5,
            Value = 5
        };


        public void ApplyToDrawableRuleset(DrawableRuleset<MaimaiHitObject> drawableRuleset)
        {
            (drawableRuleset.Playfield as MaimaiPlayfield).revolutionDuration.Value = revolutionDuration.Value;
        }
    }
}
