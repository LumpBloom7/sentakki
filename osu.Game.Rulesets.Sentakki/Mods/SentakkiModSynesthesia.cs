using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Sentakki.Localisation.Mods;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Mods;

/// <summary>
/// Mod that colours <see cref="HitObject"/>s based on the musical division they are on
/// </summary>
public class SentakkiModSynesthesia : ModSynesthesia, IApplicableToBeatmapProcessor
{
    [SettingSource(typeof(SentakkiModSynesthesiaStrings), nameof(SentakkiModSynesthesiaStrings.IntervalColouring), nameof(SentakkiModSynesthesiaStrings.IntervalColouringDescription))]
    public BindableBool IntervalColouring { get; } = new BindableBool();

    public override bool Ranked => true;

    public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
    {
        if (beatmapProcessor is not SentakkiBeatmapProcessor sbp)
            return;

        sbp.CustomNoteColouringDelegate = IntervalColouring.Value ? applyIntervalBasedNoteColouring : applyDivisorBasedNoteColouring;
    }

    private static void applyDivisorBasedNoteColouring(SentakkiBeatmap beatmap)
    {
        OsuColour colours = new OsuColour();

        foreach (var hitObject in getColorableHitObject(beatmap.HitObjects))
        {
            double startTime = getStartTime(hitObject);
            int beatDivisor = beatmap.ControlPointInfo.GetClosestBeatDivisor(startTime);
            Color4 colour = BindableBeatDivisor.GetColourFor(beatDivisor, colours);

            // Touch hold uses a palette instead of a single colour
            if (hitObject is TouchHold th)
            {
                th.ColourPalette =
                [
                    colour,
                    colour.Darken(0.7f),
                    colour,
                    colour.Darken(0.7f)
                ];
                continue;
            }

            hitObject.NoteColour = colour;

            foreach (SentakkiHitObject nested in hitObject.NestedHitObjects.OfType<SentakkiHitObject>())
                nested.NoteColour = colour;
        }
    }

    private static void applyIntervalBasedNoteColouring(SentakkiBeatmap beatmap)
    {
        OsuColour colours = new OsuColour();

        var hitobjects = getColorableHitObject(beatmap.HitObjects).OrderBy(getStartTime).ToList();

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

            double beatLength = beatmap.ControlPointInfo.TimingPointAt(currentTime).BeatLength;

            smallestDelta = double.Min(smallestDelta, beatLength);
            int divisor = (int)Math.Round(beatLength / smallestDelta);
            Color4 colour = BindableBeatDivisor.GetColourFor(divisor, colours);

            // Touch hold uses a palette instead of a single colour
            if (hitobjects[i] is TouchHold th)
            {
                th.ColourPalette =
                [
                    colour,
                    colour.Darken(0.5f),
                    colour,
                    colour.Darken(0.5f)
                ];
                continue;
            }

            hitobjects[i].NoteColour = colour;
            foreach (SentakkiHitObject nested in hitobjects[i].NestedHitObjects.OfType<SentakkiHitObject>())
                nested.NoteColour = colour;
        }
    }

    private static IEnumerable<SentakkiHitObject> getColorableHitObject(List<SentakkiHitObject> hitObjects)
    {
        foreach (var hitObject in hitObjects)
        {
            yield return hitObject;

            switch (hitObject)
            {
                case Hold h:
                    // The HitExplosion uses the colour of the hold head as well as the hold itself.
                    yield return (Hold.HoldHead)h.NestedHitObjects[0];
                    break;

                case Slide s:
                    if (s.TapType is not Slide.TapTypeEnum.None)
                        yield return s.SlideTap;

                    foreach (var slideBody in s.SlideBodies)
                        yield return slideBody;

                    break;
            }
        }
    }

    private static double getStartTime(HitObject ho)
    {
        if (ho is SlideBody sb)
            return sb.StartTime + sb.SlideBodyInfo.EffectiveWaitDuration;

        return ho.StartTime;
    }
}
