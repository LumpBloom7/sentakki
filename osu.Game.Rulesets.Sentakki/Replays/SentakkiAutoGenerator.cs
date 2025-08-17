using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Replays;

public class SentakkiAutoGenerator : AutoGenerator<SentakkiReplayFrame>
{
    public new Beatmap<SentakkiHitObject> Beatmap => (Beatmap<SentakkiHitObject>)base.Beatmap;

    public SentakkiAutoGenerator(IBeatmap beatmap)
        : base(beatmap)
    {
    }

    protected override void GenerateFrames()
    {
        Frames.Add(new SentakkiReplayFrame { Position = new Vector2(-1000), Time = -500 });

        if (Beatmap.HitObjects.Count == 0)
            return;

        List<double> frameTimings = [];

        foreach (var ho in Beatmap.HitObjects)
        {
            frameTimings.Add(ho.StartTime);

            if (ho is not IHasDuration d) continue;

            // Super short holds need a hack to ensure that sentakki's shitty auto can actually deal with them
            // The frame stable clock alone only guarantees 16ms
            if (d.Duration == 0)
                frameTimings.Add(ho.GetEndTime() + 1);
            else
                frameTimings.Add(ho.GetEndTime());
        }

        Frames.AddRange(frameTimings.Distinct().Select(t => new SentakkiReplayFrame { Position = new Vector2(-1000), Time = t }));
    }
}
