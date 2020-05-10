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
            Frames.Add(new SentakkiReplayFrame { Position = new Vector2(-1000), Time = -500 });
        }

        private Tuple<SentakkiAction, double> inUse = new Tuple<SentakkiAction, double>(SentakkiAction.Button1, -1);

        public override Replay Generate()
        {
            return Replay;
        }
    }
}
