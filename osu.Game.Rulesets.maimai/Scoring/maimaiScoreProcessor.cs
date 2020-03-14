// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Maimai.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Maimai.Judgements;

namespace osu.Game.Rulesets.Maimai.Scoring
{
    public class MaimaiScoreProcessor : ScoreProcessor
    {
        public override HitWindows CreateHitWindows() => new MaimaiHitWindows();
    }
}
