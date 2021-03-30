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
                bool isTwin = group.Count() > 1; // This determines whether the twin colour should be used

                foreach (SentakkiHitObject hitObject in group)
                {
                    if (hitObject is SentakkiLanedHitObject laned && laned.Break)
                        hitObject.NoteColour = Color4.OrangeRed;
                    else if (isTwin)
                        hitObject.NoteColour = Color4.Gold;
                    else
                        hitObject.NoteColour = hitObject.DefaultNoteColour;
                }
            }
        }
    }
}
