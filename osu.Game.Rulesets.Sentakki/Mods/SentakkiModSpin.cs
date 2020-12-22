using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModSpin : Mod, IApplicableToDrawableRuleset<SentakkiHitObject>
    {
        public override string Name => "Spin";
        public override string Description => "Replicate the true washing machine experience.";
        public override string Acronym => "S";

        public override IconUsage? Icon => FontAwesome.Solid.RedoAlt;
        public override ModType Type => ModType.Fun;

        public override bool Ranked => false;

        public override double ScoreMultiplier => 1.00;

        [SettingSource("Revolution Duration", "The duration in seconds to complete a revolution")]
        public BindableNumber<int> RevolutionDuration { get; } = new BindableNumber<int>
        {
            MinValue = 3,
            MaxValue = 10,
            Default = 5,
            Value = 5
        };

        public void ApplyToDrawableRuleset(DrawableRuleset<SentakkiHitObject> drawableRuleset)
        {
            (drawableRuleset.Playfield as SentakkiPlayfield).RevolutionDuration.Value = RevolutionDuration.Value;
        }
    }
}
