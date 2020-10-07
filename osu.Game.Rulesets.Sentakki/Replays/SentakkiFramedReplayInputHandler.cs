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
        private TouchInput getTouchInputStateOf(TouchSource touchSource)
        {
            var currTouchReplay = CurrentFrame?.TouchReplayEvents?[(int)touchSource];
            var nextTouchReplay = NextFrame?.TouchReplayEvents?[(int)touchSource];

            if (currTouchReplay is null)
            {
                return new TouchInput(new Touch(touchSource, Vector2.Zero), false);
            }
            else if (CurrentTime.Value >= currTouchReplay?.EndTime && nextTouchReplay != null)
            {
                var interpPos = Interpolation.ValueAt(CurrentTime.Value,
                currTouchReplay.PositionAtTime(CurrentTime.Value),
                nextTouchReplay.PositionAtTime(CurrentTime.Value),
                currTouchReplay.EndTime, nextTouchReplay.StartTime);

                return new TouchInput(new Touch(touchSource, GamefieldToScreenSpace(interpPos)), true);
            }
            else
            {
                return new TouchInput(new Touch(touchSource, GamefieldToScreenSpace(currTouchReplay.PositionAtTime(CurrentTime.Value))), true);
            }
        }

        public override void CollectPendingInputs(List<IInput> inputs)
        {
            //Poll touch inputs
            for (int i = 0; i < 10; ++i)
                inputs.Add(getTouchInputStateOf((TouchSource)i));

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
