using System.Collections.Generic;
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

            Color4 twinColor = Color4.Gold;
            Color4 breakColor = Color4.OrangeRed;

            var hitObjectGroups = Beatmap.HitObjects.GroupBy(h => h.StartTime).ToList();

            foreach (var group in hitObjectGroups)
            {
                bool isTwin = group.Count() > 1; // This determines whether the twin colour should be used

                List<SlideBody> slideBodiesInGroup = new List<SlideBody>();

                foreach (SentakkiHitObject hitObject in group)
                {
                    Color4 noteColor = hitObject.DefaultNoteColour;

                    if (hitObject is SentakkiLanedHitObject laned && laned.Break)
                        noteColor = breakColor;
                    else if (isTwin)
                        noteColor = twinColor;

                    // SlideTaps follow the typical coloring rules, while SlideBodies follow a different set of rules
                    if (hitObject is Slide slide)
                    {
                        slide.SlideTap.NoteColour = noteColor;
                        slideBodiesInGroup.AddRange(slide.SlideBodies);
                    }
                    else
                        hitObject.NoteColour = noteColor;
                }

                // Colour Slide bodies separately
                foreach (var slideBody in slideBodiesInGroup)
                {
                    if (slideBody.Break)
                        slideBody.NoteColour = breakColor;
                    else if (slideBodiesInGroup.Count > 1)
                        slideBody.NoteColour = twinColor;
                    else
                        slideBody.NoteColour = slideBody.DefaultNoteColour;
                }
            }
        }
    }
}
