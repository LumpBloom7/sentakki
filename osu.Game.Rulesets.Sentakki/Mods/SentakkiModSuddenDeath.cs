using System;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Sentakki.Mods;

public class SentakkiModSuddenDeath : ModSuddenDeath
{
    public override Type[] IncompatibleMods => [.. base.IncompatibleMods, typeof(SentakkiModChallenge)];
}
