using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModPerfect : ModPerfect
    {
        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
            => !(result.Judgement is IgnoreJudgement)
               && result.Type != HitResult.Great && result.Type != HitResult.Perfect;
    }
}
