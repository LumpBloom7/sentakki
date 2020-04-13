// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Maimai.Mods
{
    public class MaimaiModPerfect : ModPerfect
    {
        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
            => !(result.Judgement is IgnoreJudgement)
               && result.Type != HitResult.Great && result.Type != HitResult.Perfect;
    }
}
