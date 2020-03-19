// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Maimai.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Replays;
using osuTK;
using System.Collections.Generic;
using System.Linq;
using System;

namespace osu.Game.Rulesets.Maimai.Replays
{
    public class MaimaiAutoGenerator : AutoGenerator
    {
        protected Replay Replay;
        protected List<ReplayFrame> Frames => Replay.Frames;

        public new Beatmap<MaimaiHitObject> Beatmap => (Beatmap<MaimaiHitObject>)base.Beatmap;

        public MaimaiAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
            Replay = new Replay();
        }
        Tuple<MaimaiAction, double> inUse = new Tuple<MaimaiAction, double>(MaimaiAction.Button1, -1);
        public override Replay Generate()
        {
            Frames.Add(new MaimaiReplayFrame { Position = new Vector2(300) });
            foreach (MaimaiHitObject hitObject in Beatmap.HitObjects)
            {
                MaimaiReplayFrame currentFrame = new MaimaiReplayFrame();
                MaimaiReplayFrame nextFrame = new MaimaiReplayFrame();
                MaimaiAction nextButton = MaimaiAction.Button1;

                if (inUse.Item1 == MaimaiAction.Button1 && inUse.Item2 > hitObject.StartTime) nextButton = MaimaiAction.Button2;
                else if (inUse.Item1 == MaimaiAction.Button2 && inUse.Item2 > hitObject.StartTime) nextButton = MaimaiAction.Button1;

                bool KeepPrevious = inUse.Item2 > hitObject.StartTime;

                switch (hitObject)
                {
                    case MaimaiTouchHold th:
                        currentFrame = new MaimaiReplayFrame
                        {
                            Time = hitObject.StartTime,
                            Position = new Vector2(300),
                            noteEvent = ReplayEvent.TouchHoldDown,
                            Actions = { nextButton }
                        };
                        Frames.Add(currentFrame);
                        inUse = new Tuple<MaimaiAction, double>(nextButton, th.EndTime);
                        nextFrame = new MaimaiReplayFrame
                        {
                            Time = th.EndTime,
                            noteEvent = ReplayEvent.TouchHoldUp,
                            Position = new Vector2(300),
                        };
                        break;
                    case MaimaiHitObject tn:
                        List<MaimaiAction> startList;
                        List<MaimaiAction> endList;

                        if (KeepPrevious)
                        {
                            startList = new List<MaimaiAction>
                            {
                                nextButton,
                                inUse.Item1
                            };
                            endList = new List<MaimaiAction>
                            {
                                inUse.Item1
                            };
                        }
                        else
                        {
                            startList = new List<MaimaiAction>
                            {
                                nextButton,
                            };
                            endList = new List<MaimaiAction>();
                        }

                        currentFrame = new MaimaiReplayFrame
                        {
                            Time = tn.StartTime,
                            Position = tn.endPosition + new Vector2(300),
                            noteEvent = ReplayEvent.TapDown,
                            Actions = startList
                        };
                        Frames.Add(currentFrame);
                        nextFrame = new MaimaiReplayFrame
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
                var frame = Frames[i] as MaimaiReplayFrame;
                if (frame.noteEvent == ReplayEvent.TouchHoldDown) holdActive = true;
                else if (frame.noteEvent == ReplayEvent.TouchHoldUp) holdActive = false;

                if (holdActive && frame.noteEvent == ReplayEvent.TapUp)
                {
                    newFrames.Add(new MaimaiReplayFrame
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
