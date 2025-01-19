using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Screens.Edit;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Mods;

/// <summary>
/// Mod that colours <see cref="HitObject"/>s based on the musical division they are on
/// </summary>
public class SentakkiModQuantization : Mod, IApplicableToBeatmap, IApplicableToDrawableHitObject
{
    private readonly OsuColour colours = new OsuColour();

    private IBeatmap? currentBeatmap { get; set; }

    public override string Name => "Quantization";

    public override string Acronym => "Qt";

    public override LocalisableString Description => "Colour groups of notes based on their rate";
    public override ModType Type => ModType.Fun;
    public override Type[] IncompatibleMods => [typeof(SentakkiModSynesthesia)];

    public override double ScoreMultiplier => 0.9;

    private Dictionary<HitObject, Color4> hitObjectDeltaDivisor = new Dictionary<HitObject, Color4>();

    public void ApplyToBeatmap(IBeatmap beatmap)
    {
        //Store a reference to the current beatmap to look up the beat divisor when notes are drawn
        if (currentBeatmap != beatmap)
            currentBeatmap = beatmap;

        var hitobjects = beatmap.HitObjects.Where(h => h is Slide).SelectMany(s => s.NestedHitObjects).ToList();
        hitobjects.AddRange(beatmap.HitObjects);

        for (int i = 0; i < hitobjects.Count; ++i)
        {
            double currentTime = hitobjects[i].StartTime;

            // check the back
            double prevDelta = double.MaxValue;
            for (int j = i - 1; j >= 0; --j)
            {
                double delta = hitobjects[i].StartTime - hitobjects[j].StartTime;

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
                double delta = hitobjects[j].StartTime - hitobjects[i].StartTime;

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
            var colour = BindableBeatDivisor.GetColourFor(divisor, colours);

            hitObjectDeltaDivisor[hitobjects[i]] = colour;
        }

    }

    public void ApplyToDrawableHitObject(DrawableHitObject d)
    {
        if (currentBeatmap == null) return;

        Color4? timingBasedColour = null;

        d.HitObjectApplied += _ =>
        {
            // Slide bodies have a shootdelay, so we take into account that time too
            if (!hitObjectDeltaDivisor.TryGetValue(d.HitObject, out Color4 colour))
                return;

            timingBasedColour = colour;
        };

        // Need to set this every update to ensure it doesn't get overwritten by DrawableHitObject.OnApply() -> UpdateComboColour().
        d.OnUpdate += _ =>
        {
            if (timingBasedColour != null)
                d.AccentColour.Value = timingBasedColour.Value;
        };
    }
}

