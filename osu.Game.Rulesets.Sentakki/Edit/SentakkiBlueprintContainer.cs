using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touches;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class SentakkiBlueprintContainer : ComposeBlueprintContainer
    {
        public SentakkiBlueprintContainer(HitObjectComposer composer)
            : base(composer)
        {
        }
        protected override SelectionHandler<HitObject> CreateSelectionHandler() => new SentakkiSelectionHandler();

        public override OverlaySelectionBlueprint CreateBlueprintFor(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableTap t:
                    return new TapSelectionBlueprint(t);
                case DrawableHold h:
                    return new HoldSelectionBlueprint(h);
                case DrawableSlide s:
                    return new SlideSelectionBlueprint(s);
                case DrawableTouch t:
                    return new TouchSelectionBlueprint(t);
                case DrawableTouchHold th:
                    return new TouchHoldSelectionBlueprint(th);
            }
            return base.CreateBlueprintFor(hitObject);
        }
    }
}
