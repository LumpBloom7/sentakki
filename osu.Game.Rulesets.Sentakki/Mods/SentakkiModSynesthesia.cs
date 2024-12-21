using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Screens.Edit;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Mods;

/// <summary>
/// Mod that colours <see cref="HitObject"/>s based on the musical division they are on
/// </summary>
public class SentakkiModSynesthesia : ModSynesthesia, IApplicableToBeatmap, IApplicableToDrawableHitObject
{
    private readonly OsuColour colours = new OsuColour();

    private IBeatmap? currentBeatmap { get; set; }

    public void ApplyToBeatmap(IBeatmap beatmap)
    {
        //Store a reference to the current beatmap to look up the beat divisor when notes are drawn
        if (currentBeatmap != beatmap)
            currentBeatmap = beatmap;
    }

    public void ApplyToDrawableHitObject(DrawableHitObject d)
    {
        if (currentBeatmap == null) return;

        Color4? timingBasedColour = null;

        d.HitObjectApplied += _ =>
        {
            // Slide bodies have a shootdelay, so we take into account that time too
            double snapTime = d.HitObject.StartTime + (d is DrawableSlideBody s ? s.HitObject.ShootDelay : 0);
            timingBasedColour = BindableBeatDivisor.GetColourFor(currentBeatmap.ControlPointInfo.GetClosestBeatDivisor(snapTime), colours);
        };

        // Need to set this every update to ensure it doesn't get overwritten by DrawableHitObject.OnApply() -> UpdateComboColour().
        d.OnUpdate += _ =>
        {
            if (timingBasedColour != null)
                d.AccentColour.Value = timingBasedColour.Value;
        };
    }
}

