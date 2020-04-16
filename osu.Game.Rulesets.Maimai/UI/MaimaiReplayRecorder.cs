// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Replays;
using osu.Game.Rulesets.Maimai.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Maimai.UI
{
    public class MaimaiReplayRecorder : ReplayRecorder<MaimaiAction>
    {
        public MaimaiReplayRecorder(Replay replay)
            : base(replay)
        {
        }

        protected override ReplayFrame HandleFrame(Vector2 mousePosition, List<MaimaiAction> actions, ReplayFrame previousFrame)
            => new MaimaiReplayFrame(Time.Current, mousePosition, actions.ToArray() );
    }
}