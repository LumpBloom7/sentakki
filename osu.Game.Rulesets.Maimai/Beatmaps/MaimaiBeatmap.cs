using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Maimai.Objects;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Maimai.Beatmaps
{
    public class MaimaiBeatmap : Beatmap<MaimaiHitObject>
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
