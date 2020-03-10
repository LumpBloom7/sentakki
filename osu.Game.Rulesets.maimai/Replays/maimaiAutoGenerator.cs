// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Maimai.Objects;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Maimai.UI;
using osuTK;

namespace osu.Game.Rulesets.Maimai.Replays
{
    public class MaimaiAutoGenerator : AutoGenerator
    {
        private readonly Vector2 CENTRE = new Vector2(MaimaiPlayfield.ringSize / 2);
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
            Frames.Add(new MaimaiReplayFrame { Position = new Vector2(350) });
            foreach (MaimaiHitObject hitObject in Beatmap.HitObjects)
            {
                Frames.Add(new MaimaiReplayFrame
                {
                    Time = hitObject.StartTime,
                    Position = hitObject.endPosition + new Vector2(350)
                });
            }

            return Replay;
        }
    }
}
