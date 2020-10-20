using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModHardRock : ModHardRock, IApplicableToDrawableHitObjects
    {
        public override double ScoreMultiplier => 1.06;
        public override bool Ranked => true;

        public override bool HasImplementation => false;

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var d in drawables.OfType<DrawableSentakkiHitObject>())
            {
                switch (d)
                {
                    case DrawableHold _:
                        //hold.HitArea.Size = new Vector2(160);
                        break;

                    case DrawableTap _:
                        //tap.HitArea.Size = new Vector2(160);
                        break;
                }
            }
        }
    }
}
