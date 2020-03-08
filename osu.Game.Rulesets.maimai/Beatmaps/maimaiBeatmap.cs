using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.maimai.Objects;

namespace osu.Game.Rulesets.maimai.Beatmaps
{
    public class maimaiBeatmap : Beatmap<maimaiHitObject>
    {
        public override IEnumerable<BeatmapStatistic> GetStatistics()
        {
            int beats = HitObjects.Count;

            return new[]
            {
                new BeatmapStatistic
                {
                    Name = "Hit objects",
                    Content = beats.ToString(),
                    Icon = FontAwesome.Solid.Circle
                }
            };
        }

    }
}
