using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Sentakki.Localisation.Mods;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Screens.Edit;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Mods;

/// <summary>
/// Mod that colours <see cref="HitObject"/>s based on the musical division they are on
/// </summary>
public class SentakkiModSynesthesia : ModSynesthesia, IApplicableToBeatmapProcessor
{
    [SettingSource(typeof(SentakkiModSynesthesiaStrings), nameof(SentakkiModSynesthesiaStrings.IntervalColouring), nameof(SentakkiModSynesthesiaStrings.IntervalColouringDescription))]
    public BindableBool IntervalColouring { get; } = new BindableBool(false);

    public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
    {
        if (beatmapProcessor is not SentakkiBeatmapProcessor sbp)
            return;

        sbp.CustomNoteColouringDelegate = IntervalColouring.Value ? applyIntervalBasedNoteColouring : applyDivisorBasedNoteColouring;
    }

    private static void applyDivisorBasedNoteColouring(SentakkiBeatmap beatmap)
    {
        OsuColour colours = new OsuColour();
        foreach (var hitObject in beatmap.HitObjects)
        {
            if (hitObject is Slide slide)
            {
                foreach (SentakkiHitObject nested in slide.NestedHitObjects)
                {
                    double startTime = getStartTime(nested);
                    int beatDivisor = beatmap.ControlPointInfo.GetClosestBeatDivisor(startTime);

                    nested.NoteColour = BindableBeatDivisor.GetColourFor(beatDivisor, colours);
                }
            }

            {
                double startTime = getStartTime(hitObject);
                int beatDivisor = beatmap.ControlPointInfo.GetClosestBeatDivisor(startTime);

                hitObject.NoteColour = BindableBeatDivisor.GetColourFor(beatDivisor, colours);
            }
        }
    }

    private static void applyIntervalBasedNoteColouring(SentakkiBeatmap beatmap)
    {
        OsuColour colours = new OsuColour();

        var hitobjects = beatmap.HitObjects.Where(h => h is Slide).SelectMany(s => s.NestedHitObjects).Cast<SentakkiHitObject>().ToList();
        hitobjects.AddRange(beatmap.HitObjects);
        hitobjects = [.. hitobjects.OrderBy(getStartTime)];

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

