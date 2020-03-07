// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.maimai.Objects;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.maimai.Replays
{
    public class maimaiAutoGenerator : AutoGenerator
    {
        protected Replay Replay;
        protected List<ReplayFrame> Frames => Replay.Frames;

        public new Beatmap<maimaiHitObject> Beatmap => (Beatmap<maimaiHitObject>)base.Beatmap;

        public maimaiAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
            Replay = new Replay();
        }

        public override Replay Generate()
        {
            Frames.Add(new maimaiReplayFrame());

            foreach (maimaiHitObject hitObject in Beatmap.HitObjects)
            {
                Frames.Add(new maimaiReplayFrame
                {
                    Time = hitObject.StartTime,
                    Position = hitObject.Position,
                });
            }

            return Replay;
        }
    }
}
