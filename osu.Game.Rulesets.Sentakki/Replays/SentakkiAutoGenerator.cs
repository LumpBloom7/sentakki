using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK;

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

        public override Replay Generate()
        {
            return Replay;
        }
    }
}
