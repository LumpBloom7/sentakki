using System;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Sentakki.Mods;

public class SentakkiModSuddenDeath : ModSuddenDeath
{
    public override Type[] IncompatibleMods =>
    [
        typeof(ModNoFail),
        typeof(ModRelax),
        typeof(ModAutoplay),
        typeof(SentakkiModChallenge),
        typeof(ModPerfect)
    ];
}
