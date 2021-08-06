using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModHidden : ModHidden, IApplicableToDrawableRuleset<SentakkiHitObject>
    {
        public override string Description => "Notes fade out just before you hit them.";

        public override double ScoreMultiplier => 1.06;

        public virtual void ApplyToDrawableRuleset(DrawableRuleset<SentakkiHitObject> drawableRuleset)
        {
            SentakkiPlayfield sentakkiPlayfield = (SentakkiPlayfield)drawableRuleset.Playfield;
            LanedPlayfield lanedPlayfield = sentakkiPlayfield.LanedPlayfield;

            var lanedHitObjectArea = lanedPlayfield.LanedHitObjectArea;
            var lanedNoteProxyContainer = lanedHitObjectArea.Child;

            lanedHitObjectArea.Remove(lanedNoteProxyContainer);
            lanedHitObjectArea.Add(new CircularPlayfieldCoveringWrapper(lanedNoteProxyContainer, VisibilityReductionMode.Hidden)
            {
                CoverageRadius = 0.4f
            });
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
        }
    }
}
