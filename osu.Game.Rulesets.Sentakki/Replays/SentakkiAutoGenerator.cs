// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Replays;
using osuTK;
using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Sentakki.Replays
{
    public class SentakkiAutoGenerator : AutoGenerator
    {
        protected Replay Replay;
        protected List<ReplayFrame> Frames => Replay.Frames;

        public new Beatmap<SentakkiHitObject> Beatmap => (Beatmap<SentakkiHitObject>)base.Beatmap;

        public SentakkiAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
            Replay = new Replay();
        }

        private Tuple<SentakkiAction, double> inUse = new Tuple<SentakkiAction, double>(SentakkiAction.Button1, -1);

        public override Replay Generate()
        {
            Frames.Add(new SentakkiReplayFrame { Position = new Vector2(300) });
            foreach (SentakkiHitObject hitObject in Beatmap.HitObjects)
            {
                SentakkiReplayFrame currentFrame = new SentakkiReplayFrame();
                SentakkiReplayFrame nextFrame = new SentakkiReplayFrame();
                SentakkiAction nextButton = SentakkiAction.Button1;

                if (inUse.Item1 == SentakkiAction.Button1 && inUse.Item2 > hitObject.StartTime) nextButton = SentakkiAction.Button2;
                else if (inUse.Item1 == SentakkiAction.Button2 && inUse.Item2 > hitObject.StartTime) nextButton = SentakkiAction.Button1;

                bool KeepPrevious = inUse.Item2 > hitObject.StartTime;

                switch (hitObject)
                {
                    case TouchHold th:
                        currentFrame = new SentakkiReplayFrame
                        {
                            Time = hitObject.StartTime,
                            Position = new Vector2(300),
                            noteEvent = ReplayEvent.TouchHoldDown,
                            Actions = { nextButton }
                        };
                        Frames.Add(currentFrame);
                        inUse = new Tuple<SentakkiAction, double>(nextButton, th.EndTime);
                        nextFrame = new SentakkiReplayFrame
                        {
                            Time = th.EndTime,
                            noteEvent = ReplayEvent.TouchHoldUp,
                            Position = new Vector2(300),
                        };
                        break;

                    case Hold h:
                        currentFrame = new SentakkiReplayFrame
                        {
                            Time = hitObject.StartTime,
                            Position = h.endPosition + new Vector2(300),
                            noteEvent = ReplayEvent.HoldDown,
                            Actions = { nextButton }
                        };
                        Frames.Add(currentFrame);
                        inUse = new Tuple<SentakkiAction, double>(nextButton, h.EndTime);
                        nextFrame = new SentakkiReplayFrame
                        {
                            Time = h.EndTime,
                            noteEvent = ReplayEvent.HoldUp,
                            Position = h.endPosition + new Vector2(300)
                        };
                        break;

                    case SentakkiHitObject tn:
                        List<SentakkiAction> startList;
                        List<SentakkiAction> endList;

                        if (KeepPrevious)
                        {
                            startList = new List<SentakkiAction>
                            {
                                nextButton,
                                inUse.Item1
                            };
                            endList = new List<SentakkiAction>
                            {
                                inUse.Item1
                            };
                        }
                        else
                        {
                            startList = new List<SentakkiAction>
                            {
                                nextButton,
                            };
                            endList = new List<SentakkiAction>();
                        }

                        currentFrame = new SentakkiReplayFrame
                        {
                            Time = tn.StartTime,
                            Position = tn.endPosition + new Vector2(300),
                            noteEvent = ReplayEvent.TapDown,
                            Actions = startList
                        };
                        Frames.Add(currentFrame);
                        nextFrame = new SentakkiReplayFrame
                        {
                            Time = tn.StartTime + 1,
                            Position = tn.endPosition + new Vector2(300),
                            noteEvent = ReplayEvent.TapUp,
                            Actions = endList
                        };
                        break;
                }
                Frames.Add(nextFrame);
            }
            bool holdActive = false;
            List<ReplayFrame> newFrames = new List<ReplayFrame>();
            Frames.Sort((lhs, rhs) => lhs.Time.CompareTo(rhs.Time));
            for (int i = 0; i < Frames.Count; ++i)
            {
                var frame = Frames[i] as SentakkiReplayFrame;
                if (frame.noteEvent == ReplayEvent.TouchHoldDown) holdActive = true;
                else if (frame.noteEvent == ReplayEvent.TouchHoldUp) holdActive = false;

                if (holdActive && frame.noteEvent == ReplayEvent.TapUp)
                {
                    newFrames.Add(new SentakkiReplayFrame
                    {
                        Time = frame.Time - 2,
                        Position = new Vector2(300)
                    });
                    frame.Position = new Vector2(300);
                }
            }
            Frames.AddRange(newFrames);
            Frames.Sort((lhs, rhs) => lhs.Time.CompareTo(rhs.Time));

            return Replay;
        }
    }
}
