// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Maimai.Objects;
using osu.Game.Rulesets.Maimai.Objects.Drawables;
using osu.Game.Rulesets.Maimai.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Maimai.UI
{
    [Cached]
    public class DrawableMaimaiRuleset : DrawableRuleset<MaimaiHitObject>
    {
        public DrawableMaimaiRuleset(MaimaiRuleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
        }

        protected override Playfield CreatePlayfield() => new MaimaiPlayfield();

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new MaimaiFramedReplayInputHandler(replay);

        protected override ReplayRecorder CreateReplayRecorder(Replay replay) => new MaimaiReplayRecorder(replay);

        public override DrawableHitObject<MaimaiHitObject> CreateDrawableRepresentation(MaimaiHitObject h)
        {
            switch (h)
            {
                case Hold holdNote:
                    return new DrawableHold(holdNote);

                case TouchHold touchHold:
                    return new DrawableTouchHold(touchHold);

                case Tap tapNote:
                    return new DrawableTap(tapNote);
            }

            return null;
        }

        protected override PassThroughInputManager CreateInputManager() => new MaimaiInputManager(Ruleset?.RulesetInfo);
    }
}
