// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.maimai.Objects;
using osu.Game.Rulesets.maimai.Objects.Drawables;
using osu.Game.Rulesets.maimai.Replays;
using osu.Game.Rulesets.maimai.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.maimai.UI
{
    [Cached]
    public class DrawablemaimaiRuleset : DrawableRuleset<maimaiHitObject>
    {
        public DrawablemaimaiRuleset(maimaiRuleset ruleset, IWorkingBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
        }

        public override ScoreProcessor CreateScoreProcessor() => new maimaiScoreProcessor(this);

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new maimaiPlayfieldAdjustmentContainer();

        protected override Playfield CreatePlayfield() => new maimaiPlayfield();

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new maimaiFramedReplayInputHandler(replay);

        public override DrawableHitObject<maimaiHitObject> CreateDrawableRepresentation(maimaiHitObject h) => new DrawablemaimaiHitObject(h);

        protected override PassThroughInputManager CreateInputManager() => new maimaiInputManager(Ruleset?.RulesetInfo);
    }
}
