// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Maimai.Judgements;

namespace osu.Game.Rulesets.Maimai.Objects
{
    public class HoldTail : Tap
    {
        public override Judgement CreateJudgement() => new MaimaiJudgement();
    }
}
