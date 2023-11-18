using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sentakki.Localisation;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.UI;
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
                    Name = SentakkiBeatmapStrings.TapCount,
                    Content = taps.ToString(),
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Circles),
                },
                new BeatmapStatistic
                {
                    Name = SentakkiBeatmapStrings.HoldCount,
                    Content = holds.ToString(),
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Sliders),
                },
                new BeatmapStatistic
                {
                    Name = SentakkiBeatmapStrings.TouchHoldCount,
                    Content = touchHolds.ToString(),
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Spinners),
                },
                new BeatmapStatistic
                {
                    Name = SentakkiBeatmapStrings.TouchCount,
                    Content = touchs.ToString(),
                    CreateIcon = () => new SpriteIcon
                    {
                        Icon = OsuIcon.PlayStyleTouch,
                        Scale = new Vector2(.8f)
                    },
                },
                new BeatmapStatistic
                {
                    Name = SentakkiBeatmapStrings.SlideCount,
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
