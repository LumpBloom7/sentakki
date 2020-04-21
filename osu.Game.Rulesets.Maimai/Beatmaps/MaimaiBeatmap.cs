using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Maimai.Objects;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Maimai.Beatmaps
{
    public class MaimaiBeatmap : Beatmap<MaimaiHitObject>
    {
        public override IEnumerable<BeatmapStatistic> GetStatistics()
        {
            int beats = HitObjects.Count(b => b is Tap);
            int holds = HitObjects.Count(h => h is Hold);
            int centreHolds = HitObjects.Count(h => h is TouchHold);

            return new[]
            {
                new BeatmapStatistic
                {
                    Name = "Tap Count",
                    Content = beats.ToString(),
                    Icon = FontAwesome.Solid.Circle
                },
                new BeatmapStatistic
                {
                    Name = "Hold Count",
                    Content = holds.ToString(),
                    Icon = FontAwesome.Solid.Circle
                },
                new BeatmapStatistic
                {
                    Name = "Spinner Hold Count",
                    Content = centreHolds.ToString(),
                    Icon = FontAwesome.Solid.Circle
                }
            };
        }
    }
}
