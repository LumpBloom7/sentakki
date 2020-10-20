using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Beatmaps
{
    public class SentakkiBeatmapProcessor : BeatmapProcessor
    {
        private const int stack_distance = 3;

        public SentakkiBeatmapProcessor(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        public override void PreProcess()
        {
            var hitObjectGroups = Beatmap.HitObjects.GroupBy(h => h.StartTime);
            foreach (var group in hitObjectGroups)
            {
                int count = group.Count();
                foreach (SentakkiHitObject h in group)
                {
                    h.HasTwin = count > 1;
                }
            }
        }
    }
}
