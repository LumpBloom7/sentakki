using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModAutoTouch : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Auto Touch";
        public override string Acronym => "AT";
        public override IconUsage? Icon => OsuIcon.PlaystyleTouch;
        public override ModType Type => ModType.Automation;
        public override string Description => @"Focus on the laned notes. Touch screen notes will be completed automatically.";
        public override double ScoreMultiplier => .5f;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModAutoplay)).ToArray();

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var d in drawables.OfType<DrawableSentakkiHitObject>())
                switch (d)
                {
                    case DrawableSlide _:
                    case DrawableTouch _:
                    case DrawableTouchHold _:
                    // Slide nodes needs to be handled as well because the pool creates the object outside the DHO context
                    case DrawableSlideNode _:
                        d.Auto = true;
                        break;
                }
        }
    }
}
