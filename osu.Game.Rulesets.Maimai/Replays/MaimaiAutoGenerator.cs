// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Maimai.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Replays;
using osuTK;
using System.Collections.Generic;

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

        public override Replay Generate()
        {
            Frames.Add(new MaimaiReplayFrame { Position = new Vector2(300) });
            foreach (MaimaiHitObject hitObject in Beatmap.HitObjects)
            {
                MaimaiReplayFrame currentFrame = new MaimaiReplayFrame();
                MaimaiReplayFrame nextFrame = new MaimaiReplayFrame();

                switch (hitObject)
                {
                    case MaimaiTouchHold th:
                        currentFrame = new MaimaiReplayFrame
                        {
                            Time = hitObject.StartTime,
                            Position = new Vector2(300),
                        };
                        currentFrame.Actions.Add(MaimaiAction.Button1);
                        nextFrame = new MaimaiReplayFrame
                        {
                            Time = th.EndTime + 1,
                            Position = new Vector2(300),
                        };
                        break;
                    case MaimaiHitObject tn:
                        currentFrame = new MaimaiReplayFrame
                        {
                            Time = tn.StartTime,
                            Position = tn.endPosition + new Vector2(300),
                        };
                        currentFrame.Actions.Add(MaimaiAction.Button1);
                        nextFrame = new MaimaiReplayFrame
                        {
                            Time = tn.StartTime + 1,
                            Position = tn.endPosition + new Vector2(300),
                        };
                        break;
                }
                Frames.Add(currentFrame);
                Frames.Add(nextFrame);

            }

            return Replay;
        }
    }
}
