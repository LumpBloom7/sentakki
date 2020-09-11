using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;
using System.Collections.Generic;
using System.Linq;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Beatmaps
{
    public class SentakkiBeatmap : Beatmap<SentakkiHitObject>
    {
        public override IEnumerable<BeatmapStatistic> GetStatistics()
        {
            int taps = HitObjects.Count(b => b is Tap);
            int holds = HitObjects.Count(h => h is Hold);
            int touchHolds = HitObjects.Count(h => h is TouchHold);
            int touchs = HitObjects.Count(h => h is Touch);
            int slides = HitObjects.Count(h => h is Slide);

            return new[]
            {
                new BeatmapStatistic
                {
                    Name = "Tap Count",
                    Content = taps.ToString(),
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Circles),
                },
                new BeatmapStatistic
                {
                    Name = "Hold Count",
                    Content = holds.ToString(),
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Sliders),
                },
                new BeatmapStatistic
                {
                    Name = "TouchHold Count",
                    Content = touchHolds.ToString(),
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Spinners),
                },
                new BeatmapStatistic
                {
                    Name = "Touch Count",
                    Content = touchs.ToString(),
                    CreateIcon = () => new SpriteIcon
                    {
                        Icon = FontAwesome.Regular.HandPointRight,
                        Scale = new Vector2(.7f)
                    },
                },
                new BeatmapStatistic
                {
                    Name = "Slide Count",
                    Content = slides.ToString(),
                    CreateIcon = () => new SpriteIcon
                    {
                        Icon = FontAwesome.Regular.Star,
                        Scale = new Vector2(.7f)
                    },
                },
            };
        }
    }
}
