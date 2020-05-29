using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModFadeIn : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Fade In";
        public override string Acronym => "FI";
        public override IconUsage? Icon => OsuIcon.ModHidden;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => @"Notes appear out of nowhere!";
        public override double ScoreMultiplier => 1.06;
        public override bool Ranked => true;

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var d in drawables.OfType<DrawableSentakkiHitObject>())
            {
                d.IsFadeIn = true;
            }
        }
    }
}
