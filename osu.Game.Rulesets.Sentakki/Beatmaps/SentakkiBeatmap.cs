using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Sentakki.Beatmaps
{
    public class SentakkiBeatmap : Beatmap<SentakkiHitObject>
    {
        public override IEnumerable<BeatmapStatistic> GetStatistics()
        {
            int taps = HitObjects.Count(b => b is Tap);
            int holds = HitObjects.Count(h => h is Hold);
            int centreHolds = HitObjects.Count(h => h is TouchHold);
            int touchs = HitObjects.Count(h => h is Touch);
            int slides = HitObjects.Count(h => h is Slide);

            return new[]
            {
                new BeatmapStatistic
                {
                    Name = "Tap Count",
                    Content = taps.ToString(),
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
                },
                new BeatmapStatistic
                {
                    Name = "Touch Count",
                    Content = touchs.ToString(),
                    Icon = FontAwesome.Solid.Circle
                },
                new BeatmapStatistic
                {
                    Name = "Slide Count",
                    Content = slides.ToString(),
                    Icon = FontAwesome.Solid.Circle
                },
            };
        }
    }
}
