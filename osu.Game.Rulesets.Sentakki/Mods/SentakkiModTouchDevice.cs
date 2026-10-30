using System;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Sentakki.Mods;

public class SentakkiModTouchDevice : ModTouchDevice
{
    public override Type[] IncompatibleMods => [typeof(SentakkiModRelax)];

    // Touchscreen plays are legit in sentakki
    public override bool Ranked => true;
}
