using System;
using System.Linq;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModNoFail : ModNoFail
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(SentakkiModChallenge)).ToArray();
    }
}
