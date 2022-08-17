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
                        slide.SlideTap.NoteColour = noteColor;
                    else
                        hitObject.NoteColour = noteColor;
                }

                // Slide bodies can't be break coloured, but can use the twin colour
                // However, the twin colour is only used if there exists another SlideBody with the same start and end time
                var slideBodies = group.OfType<Slide>().SelectMany(s => s.SlideBodies).GroupBy(sb => sb.EndTime).ToList();

                foreach (var slideBodyGroup in slideBodies)
                {
                    bool isTwinSlide = slideBodyGroup.Count() > 1;

                    foreach (var slideBody in slideBodyGroup)
                        slideBody.NoteColour = isTwinSlide ? Color4.Gold : slideBody.DefaultNoteColour;
                }
            }
        }
    }
}
