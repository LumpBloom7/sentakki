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

            var hitObjectGroups = Beatmap.HitObjects.GroupBy(h => h.StartTime).ToList();

            foreach (var group in hitObjectGroups)
            {
                bool isTwin = group.Count() > 1; // This determines whether the twin colour should be used

                foreach (SentakkiHitObject hitObject in group)
                {
                    Color4 noteColor = hitObject.DefaultNoteColour;

                    if (hitObject is SentakkiLanedHitObject laned && laned.Break)
                        noteColor = Color4.OrangeRed;
                    else if (isTwin)
                        noteColor = Color4.Gold;

                    // SlideTaps follow the typical coloring rules, while SlideBodies follow a different set of rules
                    if (hitObject is Slide slide)
                    {
                        slide.SlideTap.NoteColour = noteColor;
                        if (group.Any(h => h is Slide && h != slide))
                            foreach (var sBody in slide.SlideBodies)
                                sBody.NoteColour = Color4.Gold;
                    }
                    else
                        hitObject.NoteColour = noteColor;
                }
            }
        }
    }
}
