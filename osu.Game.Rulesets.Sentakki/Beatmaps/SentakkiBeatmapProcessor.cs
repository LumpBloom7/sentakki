using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Beatmaps
{
    public class SentakkiBeatmapProcessor : BeatmapProcessor
    {
        public new SentakkiBeatmap Beatmap => (SentakkiBeatmap)((base.Beatmap is EditorBeatmap eb) ? eb.PlayableBeatmap : base.Beatmap);

        public Action<SentakkiBeatmap>? CustomNoteColouringDelegate = null;

        public SentakkiBeatmapProcessor(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        public override void PostProcess()
        {
            base.PostProcess();

            if (CustomNoteColouringDelegate is not null)
                CustomNoteColouringDelegate?.Invoke(Beatmap);
            else
                applyDefaultNoteColouring();
        }

        private void applyDefaultNoteColouring()
        {
            Color4 twinColor = Color4.Gold;
            Color4 breakColor = Color4.OrangeRed;

            var hitObjectGroups = getColorableHitObject(Beatmap.HitObjects).GroupBy(h => new { isSlide = h is SlideBody, time = h.StartTime + (h is SlideBody s ? s.ShootDelay : 0) });

            foreach (var group in hitObjectGroups)
            {
                bool isTwin = group.Count(countsForTwin) > 1; // This determines whether the twin colour should be used for eligible objects

                foreach (SentakkiHitObject hitObject in group)
                {
                    if (hitObject is TouchHold th)
                    {
                        th.ColourPalette = th.Break ? TouchHold.BreakPalette : TouchHold.DefaultPalette;
                        continue;
                    }

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
            for (int i = 0; i < hitObjects.Count; ++i)
            {
                var hitObject = hitObjects[i];
                if (canBeColored(hitObject)) yield return hitObject;

                foreach (var nested in getColorableHitObject(hitObject.NestedHitObjects).AsEnumerable())
                    yield return nested;
            }
        }

        private static bool canBeColored(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Tap:
                case SlideBody:
                case Hold.HoldHead:
                case Touch:
                case TouchHold:
                    return true;

                // HitObject lines take the parent colour, instead of considering the nested object's colour
                case Slide:
                case Hold:
                    return true;
            }
            return false;
        }

        private static bool countsForTwin(HitObject hitObject) => hitObject switch
        {
            Hold.HoldHead => false,
            TouchHold => false,
            Slide => false,
            _ => true
        };
    }
}
