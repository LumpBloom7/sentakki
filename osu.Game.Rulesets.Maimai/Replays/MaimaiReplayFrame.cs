// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;
using osuTK;

namespace osu.Game.Rulesets.Maimai.Replays
{
    public class MaimaiReplayFrame : ReplayFrame, IConvertibleReplayFrame
    {
        public ReplayEvent noteEvent = ReplayEvent.none;
        public Vector2 Position;
        public List<MaimaiAction> Actions = new List<MaimaiAction>();

        public MaimaiReplayFrame()
        {
        }

        public MaimaiReplayFrame(double time, Vector2 position, params MaimaiAction[] actions)
            : base(time)
        {
            Position = position;
            Actions.AddRange(actions);
        }

        public void FromLegacy(LegacyReplayFrame currentFrame, IBeatmap beatmap, ReplayFrame lastFrame = null)
        {
            Position = currentFrame.Position;

            if (currentFrame.MouseLeft) Actions.Add(MaimaiAction.Button1);
            if (currentFrame.MouseRight) Actions.Add(MaimaiAction.Button2);
        }

        public LegacyReplayFrame ToLegacy(IBeatmap beatmap)
        {
            ReplayButtonState state = ReplayButtonState.None;

            if (Actions.Contains(MaimaiAction.Button1)) state |= ReplayButtonState.Left1;
            if (Actions.Contains(MaimaiAction.Button2)) state |= ReplayButtonState.Right1;

            return new LegacyReplayFrame(Time, Position.X, Position.Y, state);
        }
    }

    public enum ReplayEvent
    {
        none,
        TapDown,
        TapUp,
        TouchHoldDown,
        TouchHoldUp,
        HoldDown,
        HoldUp
    }
}
