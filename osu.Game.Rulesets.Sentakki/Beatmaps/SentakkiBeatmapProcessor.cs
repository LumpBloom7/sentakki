using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Beatmaps
{
    public class SentakkiBeatmapProcessor : BeatmapProcessor
    {
        public NoteColouringMode ColouringMode = NoteColouringMode.DEFAULT;

        public new SentakkiBeatmap Beatmap => (SentakkiBeatmap)base.Beatmap;
        public SentakkiBeatmapProcessor(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        public override void PostProcess()
        {
            base.PostProcess();

            switch (ColouringMode)
            {
                case NoteColouringMode.GC_BASED:
                    applyGCBasedNoteColouring();
                    break;
                case NoteColouringMode.DIVISOR_BASED:
                    applyDivisorBasedNoteColouring();
                    break;
                default:
                    applyDefaultNoteColouring();
                    break;
            }
        }

        public enum NoteColouringMode
        {
            DEFAULT,
            DIVISOR_BASED,
            GC_BASED
        }

        public void applyDefaultNoteColouring()
        {
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

        private void applyDivisorBasedNoteColouring()
        {
            OsuColour colours = new OsuColour();
            foreach (var hitObject in Beatmap.HitObjects)
            {
                if (hitObject is Slide slide)
                {
                    foreach (SentakkiHitObject nested in slide.NestedHitObjects)
                    {
                        double startTime = getStartTime(nested);
                        int beatDivisor = Beatmap.ControlPointInfo.GetClosestBeatDivisor(startTime);

                        nested.NoteColour = BindableBeatDivisor.GetColourFor(beatDivisor, colours);
                    }
                }

                {
                    double startTime = getStartTime(hitObject);
                    int beatDivisor = Beatmap.ControlPointInfo.GetClosestBeatDivisor(startTime);

                    hitObject.NoteColour = BindableBeatDivisor.GetColourFor(beatDivisor, colours);
                }
            }
        }

        private void applyGCBasedNoteColouring()
        {
            OsuColour colours = new OsuColour();

            var hitobjects = Beatmap.HitObjects.Where(h => h is Slide).SelectMany(s => s.NestedHitObjects).Cast<SentakkiHitObject>().ToList();
            hitobjects.AddRange(Beatmap.HitObjects);
            hitobjects = [.. hitobjects.OrderBy(h => h.StartTime)];

            for (int i = 0; i < hitobjects.Count; ++i)
            {
                double currentTime = getStartTime(hitobjects[i]);

                // check the back
                double prevDelta = double.MaxValue;
                for (int j = i - 1; j >= 0; --j)
                {
                    double delta = currentTime - getStartTime(hitobjects[j]);

                    if (delta >= double.Epsilon)
                    {
                        prevDelta = delta;
                        break;
                    }
                }

                // check the front
                double frontDelta = double.MaxValue;
                for (int j = i + 1; j < hitobjects.Count; ++j)
                {
                    double delta = getStartTime(hitobjects[j]) - currentTime;

                    if (delta >= double.Epsilon)
                    {
                        frontDelta = delta;
                        break;
                    }
                }

                double smallestDelta = double.Min(prevDelta, frontDelta);

                double beatLength = Beatmap.ControlPointInfo.TimingPointAt(currentTime).BeatLength;

                smallestDelta = double.Min(smallestDelta, beatLength);
                int divisor = (int)Math.Round(beatLength / smallestDelta);
                var colour = BindableBeatDivisor.GetColourFor(divisor, colours);

                hitobjects[i].NoteColour = colour;
            }
        }

        private static double getStartTime(HitObject ho)
        {
            if (ho is SlideBody sb)
                return sb.StartTime + sb.ShootDelay;

            return ho.StartTime;
        }
    }
}
