using System;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Localisation.Mods;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;

namespace osu.Game.Rulesets.Sentakki.Mods;

public class SentakkiModNoTouch : Mod, IApplicableToDrawableHitObject
{
    public override string Name => "No Touch";
    public override string Acronym => "NT";
    public override ModType Type => ModType.Automation;
    public override LocalisableString Description => SentakkiModNoTouchStrings.ModDescription;
    public override double ScoreMultiplier => .2f;
    public override Type[] IncompatibleMods => [.. base.IncompatibleMods, typeof(ModAutoplay)];

    public void ApplyToDrawableHitObject(DrawableHitObject drawableHitObject)
    {
        if (drawableHitObject is not DrawableSentakkiHitObject drawableSentakkiHitObject) return;

        switch (drawableSentakkiHitObject)
        {
            case DrawableSlide:
            case DrawableTouch:
            case DrawableTouchHold:
            // Slide nodes needs to be handled as well because the pool creates the object outside the DHO context
            case DrawableSlideCheckpointNode:
                drawableSentakkiHitObject.Auto = true;
                break;
        }
    }
}
