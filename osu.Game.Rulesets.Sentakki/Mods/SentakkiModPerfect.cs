using System;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModPerfect : ModPerfect
    {
        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
            => !(result.Judgement is IgnoreJudgement)
               && result.Type < result.Judgement.MaxResult;

        public override Type[] IncompatibleMods => new Type[4]
        {
                typeof(ModNoFail),
                typeof(ModRelax),
                typeof(ModAutoplay),
                typeof(SentakkiModChallenge)
        };
    }
}
