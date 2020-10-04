using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Replays.Types;
using osu.Framework.Input;
using osuTK;
using osu.Framework.Input.Events;

namespace osu.Game.Rulesets.Sentakki.Replays
{
    public class SentakkiReplayFrame : ReplayFrame, IConvertibleReplayFrame
    {
        public Vector2 Position;
        public List<SentakkiAction> Actions = new List<SentakkiAction>();

        public TouchReplayEvent[] TouchReplayEvents { get; set; }

        public SentakkiReplayFrame()
        {
        }

        public SentakkiReplayFrame(double time, Vector2 position, TouchReplayEvent[] TRE, params SentakkiAction[] actions)
            : base(time)
        {
            Position = position;
            TouchReplayEvents = TRE;
            Actions.AddRange(actions);
        }

        public void FromLegacy(LegacyReplayFrame currentFrame, IBeatmap beatmap, ReplayFrame lastFrame = null)
        {
            Position = currentFrame.Position;

            if (currentFrame.MouseLeft) Actions.Add(SentakkiAction.Button1);
            if (currentFrame.MouseRight) Actions.Add(SentakkiAction.Button2);
        }

        public LegacyReplayFrame ToLegacy(IBeatmap beatmap)
        {
            ReplayButtonState state = ReplayButtonState.None;

            if (Actions.Contains(SentakkiAction.Button1)) state |= ReplayButtonState.Left1;
            if (Actions.Contains(SentakkiAction.Button2)) state |= ReplayButtonState.Right1;

            return new LegacyReplayFrame(Time, Position.X, Position.Y, state);
        }
    }
    public class TouchReplayEvent
    {
        public TouchReplayEvent(Vector2 Position, double Duration, double startTime, float rotation = 0)
        {
            MovementPath = new SliderPath(new PathControlPoint[]{
                new PathControlPoint(Position)
            });
            this.Duration = Duration;
            StartTime = startTime;
            Rotation = rotation;
        }

        public TouchReplayEvent(SliderPath path, double Duration, double startTime, float rotation = 0)
        {
            MovementPath = path;
            this.Duration = Duration;
            StartTime = startTime;
            Rotation = rotation;
        }


        public SliderPath MovementPath;
        public double Duration;
        public double StartTime;
        public float Rotation = 0;

    }
}
