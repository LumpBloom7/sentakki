using osu.Framework.Input.StateChanges;
using osu.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Utils;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;
using osuTK;
using System;

namespace osu.Game.Rulesets.Sentakki.Replays
{
    public class SentakkiFramedReplayInputHandler : FramedReplayInputHandler<SentakkiReplayFrame>
    {
        public SentakkiFramedReplayInputHandler(Replay replay)
            : base(replay)
        {
        }

        protected override bool IsImportant(SentakkiReplayFrame frame) => true;
        public bool UsingSensorMode => CurrentFrame.UsingSensorMode;

        protected Vector2 Position
        {
            get
            {
                var frame = CurrentFrame;

                if (frame == null)
                    return Vector2.Zero;

                Debug.Assert(CurrentTime != null);

                return NextFrame != null ? Interpolation.ValueAt(CurrentTime.Value, frame.Position, NextFrame.Position, frame.Time, NextFrame.Time) : frame.Position;
            }
        }

        public override void CollectPendingInputs(List<IInput> inputs)
        {
            if (CurrentFrame != null && CurrentFrame.TouchReplayEvents != null)
            {
                for (int i = 0; i < 10; ++i)
                {
                    var activeTouchPoint = CurrentFrame.TouchReplayEvents[i];
                    if (activeTouchPoint == null)
                    {
                        inputs.Add(new TouchInput(new Touch((TouchSource)i, Vector2.Zero), false));
                    }
                    else
                    {
                        //get point position
                        double percentage = Math.Clamp(Interpolation.ValueAt(CurrentTime.Value, 0D, 1D, activeTouchPoint.StartTime, activeTouchPoint.StartTime + activeTouchPoint.Duration), 0, 1);

                        var position = activeTouchPoint.MovementPath.PositionAt(percentage);

                        position = SentakkiExtensions.RotatePointAroundOrigin(position, Vector2.Zero, activeTouchPoint.Rotation) + new Vector2(300);

                        inputs.Add(new TouchInput(new Touch((TouchSource)i, GamefieldToScreenSpace(position)), true));
                    }
                }
            }

            inputs.Add(new MousePositionAbsoluteInput
            {
                Position = GamefieldToScreenSpace(Position),
            });

            inputs.Add(new ReplayState<SentakkiAction>
            {
                PressedActions = CurrentFrame?.Actions ?? new List<SentakkiAction>(),
            });
        }
    }
}
