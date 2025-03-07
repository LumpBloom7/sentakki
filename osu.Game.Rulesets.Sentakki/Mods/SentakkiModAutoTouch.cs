using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Localisation.Mods;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModAutoTouch : Mod, IApplicableToDrawableHitObject
    {
        public override string Name => "Auto Touch";
        public override string Acronym => "AT";
        public override IconUsage? Icon => OsuIcon.PlayStyleTouch;
        public override ModType Type => ModType.Automation;
        public override LocalisableString Description => SentakkiModAutoTouchStrings.ModDescription;
        public override double ScoreMultiplier => .2f;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModAutoplay)).ToArray();

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
}
