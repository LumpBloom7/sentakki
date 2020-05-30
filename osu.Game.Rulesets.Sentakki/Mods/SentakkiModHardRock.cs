using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModHardRock : ModHardRock, IApplicableToDrawableHitObjects
    {
        public override double ScoreMultiplier => 1.06;
        public override bool Ranked => true;

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var d in drawables.OfType<DrawableSentakkiHitObject>())
            {
                switch (d)
                {
                    case DrawableHold hold:
                        hold.HitArea.Size = new Vector2(160);
                        break;

                    case DrawableTap tap:
                        tap.HitArea.Size = new Vector2(160);
                        break;
                }
            }
        }
    }
}
