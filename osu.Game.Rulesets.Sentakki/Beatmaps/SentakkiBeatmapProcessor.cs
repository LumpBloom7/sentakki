using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
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

            var hitObjectGroups = getColorableHitObject(Beatmap.HitObjects).GroupBy(h => new { isSlide = h is SlideBody, time = h.StartTime + (h is SlideBody s ? s.ShootDelay : 0) });

            foreach (var group in hitObjectGroups)
            {
                bool isTwin = group.Count() > 1; // This determines whether the twin colour should be used

                foreach (SentakkiHitObject hitObject in group)
                {
                    Color4 noteColor = hitObject.DefaultNoteColour;

                    if (hitObject is SentakkiLanedHitObject laned && laned.Break)
                        noteColor = breakColor;
                    else if (isTwin)
                        noteColor = twinColor;

                    hitObject.NoteColour = noteColor;
                }
            }
        }

        private IEnumerable<HitObject> getColorableHitObject(IReadOnlyList<HitObject> hitObjects)
        {
            foreach (var hitObject in hitObjects)
            {
                if (canBeColored(hitObject)) yield return hitObject;

                foreach (var nested in getColorableHitObject(hitObject.NestedHitObjects).AsEnumerable())
                    yield return nested;
            }
        }

        private bool canBeColored(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Tap:
                case SlideBody:
                case Hold:
                case Touch:
                    return true;
            }
            return false;
        }
    }
}
