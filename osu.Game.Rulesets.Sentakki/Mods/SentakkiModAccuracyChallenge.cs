using System;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Sentakki.Mods;

public class SentakkiModAccuracyChallenge : ModAccuracyChallenge
{
    public override Type[] IncompatibleMods => [.. base.IncompatibleMods, typeof(ModFailCondition)];
}
