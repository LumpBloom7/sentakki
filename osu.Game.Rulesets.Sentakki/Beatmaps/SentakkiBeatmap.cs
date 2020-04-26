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
            int beats = HitObjects.Count(b => b.GetType() == typeof(Tap));
            int holds = HitObjects.Count(h => h is Hold);
            int centreHolds = HitObjects.Count(h => h is TouchHold);
            int breaks = HitObjects.Count(h => h is Break);

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
                },
                new BeatmapStatistic
                {
                    Name = "Break Count",
                    Content = breaks.ToString(),
                    Icon = FontAwesome.Solid.Circle
                },
            };
        }
    }
}
