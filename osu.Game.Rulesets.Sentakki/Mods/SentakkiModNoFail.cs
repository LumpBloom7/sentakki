using System;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModNoFail : ModNoFail
    {
        public override Type[] IncompatibleMods => new Type[]{
            typeof(ModRelax),
            typeof(ModFailCondition),
            typeof(SentakkiModChallenge),
            typeof(ModAutoplay)
        };
    }
}
