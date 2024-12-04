using System;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Sentakki.Mods;

public class SentakkiModTouchDevice : ModTouchDevice
{
    public override Type[] IncompatibleMods => Array.Empty<Type>();
}
