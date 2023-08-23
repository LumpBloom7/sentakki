using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Localisation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Framework.Graphics.Sprites;
using System;
using System.ComponentModel;

namespace osu.Game.Rulesets.Sentakki.Mods;

public class SentakkiModGori : Mod, IApplicableToDrawableHitObject
{
    public override string Name => "Gori";
    public override string Acronym => "GR";

    public override bool HasImplementation => true;
    public override double ScoreMultiplier => 1;

    public override IconUsage? Icon => FontAwesome.Solid.PooStorm;

    public override Type[] IncompatibleMods => new Type[]{
        typeof(ModAutoplay),
    };

    // TODO: LOCALISATION
    public override LocalisableString Description => "You either hit it right, or you don't hit it at all.";

    [SettingSource("Lowest valid hit result", "The minimum HitResult that is accepted during gameplay. Anything below will be considered a miss.")]
    public Bindable<SentakkiHitResult> MaxHitResult { get; } = new Bindable<SentakkiHitResult>(SentakkiHitResult.Perfect);

    public void ApplyToDrawableHitObject(DrawableHitObject drawable)
    {
        if (drawable is DrawableSentakkiHitObject d)
            d.MinimumAcceptedHitResult = (HitResult)MaxHitResult.Value;
    }

    public enum SentakkiHitResult
    {
        Great = 4,
        Perfect = 5,
        [Description("Critical Perfect")] Critical = 6,
    }
}
