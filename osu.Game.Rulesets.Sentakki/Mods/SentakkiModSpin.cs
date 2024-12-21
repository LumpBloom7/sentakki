using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sentakki.Localisation.Mods;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public partial class SentakkiModSpin : Mod, IUpdatableByPlayfield
    {
        public override string Name => "Spin";
        public override LocalisableString Description => SentakkiModSpinStrings.ModDescription;

        public override string ExtendedIconInformation => SentakkiModSpinStrings.RevolutionDurationTooltip(seconds: RevolutionDuration.Value).ToString();
        public override string Acronym => "S";

        public override IconUsage? Icon => FontAwesome.Solid.RedoAlt;
        public override ModType Type => ModType.Fun;

        public override double ScoreMultiplier => 1.00;

        [SettingSource(
            typeof(SentakkiModSpinStrings),
            nameof(SentakkiModSpinStrings.RevolutionDuration),
            nameof(SentakkiModSpinStrings.RevolutionDurationDescription),
            SettingControlType = typeof(SettingsSlider<int, RevolutionDurationSlider>))]
        public BindableNumber<int> RevolutionDuration { get; } = new BindableNumber<int>
        {
            MinValue = 3,
            MaxValue = 10,
            Default = 5,
            Value = 5
        };

        public void Update(Playfield playfield)
        {
            // We only rotate the main playfield
            if (playfield is SentakkiPlayfield)
                playfield.Rotation = (float)(playfield.Time.Current / (RevolutionDuration.Value * 1000)) * 360f;
        }

        public partial class RevolutionDurationSlider : RoundedSliderBar<int>
        {
            public override LocalisableString TooltipText => SentakkiModSpinStrings.RevolutionDurationTooltip(Current.Value);
        }
    }
}
