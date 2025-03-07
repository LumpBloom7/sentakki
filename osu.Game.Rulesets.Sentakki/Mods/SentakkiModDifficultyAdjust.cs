using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sentakki.Localisation.Mods;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Mods;

public class SentakkiModDifficultyAdjust : Mod, IApplicableAfterBeatmapConversion
{
    public override string Name => "Difficulty Adjust";

    public override LocalisableString Description => "Override a beatmap's difficulty settings.";

    public override string Acronym => "DA";

    public override ModType Type => ModType.Conversion;

    public override IconUsage? Icon => FontAwesome.Solid.Hammer;

    public override double ScoreMultiplier
    {
        get
        {
            double result = 1;

            if (BreakRemoval.Value)
                result *= 0.8;

            if (AllEx.Value)
                result *= 0.3;

            return result;
        }
    }

    public override bool RequiresConfiguration => true;
    public override bool Ranked => true;

    [SettingSource(typeof(SentakkiModDifficultyAdjustStrings), nameof(SentakkiModDifficultyAdjustStrings.BreakRemoval))]
    public BindableBool BreakRemoval { get; } = new BindableBool();

    [SettingSource(typeof(SentakkiModDifficultyAdjustStrings), nameof(SentakkiModDifficultyAdjustStrings.ExRemoval))]
    public BindableBool ExRemoval { get; } = new BindableBool();

    [SettingSource(typeof(SentakkiModDifficultyAdjustStrings), nameof(SentakkiModDifficultyAdjustStrings.AllEx))]
    public BindableBool AllEx { get; } = new BindableBool();

    public SentakkiModDifficultyAdjust()
    {
        ExRemoval.BindValueChanged(v =>
        {
            if (v.NewValue)
                AllEx.Value = false;
        });

        AllEx.BindValueChanged(v =>
        {
            if (v.NewValue)
                ExRemoval.Value = false;
        });
    }

    public void ApplyToBeatmap(IBeatmap beatmap)
    {
        foreach (var hitObject in beatmap.HitObjects)
        {
            if (hitObject is not SentakkiHitObject sho)
                continue;

            sho.Break = !BreakRemoval.Value && sho.Break;
            sho.Ex = AllEx.Value || (!ExRemoval.Value && sho.Ex);

            if (sho is not Slide slide)
                continue;

            foreach (var slideInfo in slide.SlideInfoList)
            {
                slideInfo.Break = !BreakRemoval.Value && slideInfo.Break;
                slideInfo.Ex = AllEx.Value || (!ExRemoval.Value && slideInfo.Ex);
            }
        }
    }
}
