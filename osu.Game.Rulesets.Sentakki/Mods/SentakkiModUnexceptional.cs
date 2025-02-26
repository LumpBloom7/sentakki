using System;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Mods;

public class SentakkiModUnexceptional : Mod, IApplicableAfterBeatmapConversion
{
    public override Type[] IncompatibleMods =>
    [
        typeof(SentakkiModRelax)
    ];

    public override string Name => "Unexceptional";

    public override string Acronym => "UE";
    public override ModType Type => ModType.DifficultyIncrease;

    public override LocalisableString Description => "All EX notes are now regular notes.";

    public override double ScoreMultiplier => 1;

    public void ApplyToBeatmap(IBeatmap beatmap)
    {
        foreach (SentakkiHitObject ho in beatmap.HitObjects)
            ho.Ex = false;
    }
}
