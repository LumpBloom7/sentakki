using System;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Mods;

public class SentakkiModNoBreak : Mod, IApplicableAfterBeatmapConversion
{
    public override string Name => "BREAKless";

    public override string Acronym => "BL";
    public override ModType Type => ModType.DifficultyReduction;

    public override LocalisableString Description => "Removes the BREAK modifier from all hitobjects";

    public override double ScoreMultiplier => 0.9;

    public void ApplyToBeatmap(IBeatmap beatmap)
    {
        foreach (SentakkiLanedHitObject ho in beatmap.HitObjects)
        {
            ho.Break = false;

            if (ho is Slide slide)
            {
                foreach (var si in slide.SlideInfoList)
                {
                    si.Break = false;
                }
            }
        }
    }
}
