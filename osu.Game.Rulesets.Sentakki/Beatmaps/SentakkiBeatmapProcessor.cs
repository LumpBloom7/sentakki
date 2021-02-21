using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Beatmaps
{
    public class SentakkiBeatmapProcessor : BeatmapProcessor
    {
        public SentakkiBeatmapProcessor(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        public override void PostProcess()
        {
            base.PostProcess();
            var hitObjectGroups = Beatmap.HitObjects.GroupBy(h => h.StartTime);
            foreach (var group in hitObjectGroups)
            {
                int count = group.Count(); // This determines whether the twin colour should be used

                foreach (SentakkiHitObject hitObject in group)
                {
                    if (hitObject is SentakkiLanedHitObject laned && laned.Break) // Break has top priority
                        hitObject.ColourBindable.Value = Color4.OrangeRed;
                    else if (count > 1)
                        hitObject.ColourBindable.Value = Color4.Gold;
                }
            }
        }
    }
}
