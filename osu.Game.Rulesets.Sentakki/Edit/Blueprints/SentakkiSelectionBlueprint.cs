using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints
{
    public abstract class SentakkiSelectionBlueprint<T> : OverlaySelectionBlueprint
        where T : SentakkiHitObject
    {
        protected new T HitObject => (T)DrawableObject.HitObject;

        protected override bool AlwaysShowWhenSelected => true;

        protected override bool ShouldBeAlive =>
            (DrawableObject.IsAlive && DrawableObject.IsPresent && (DrawableObject as DrawableSentakkiHitObject).IsVisible) || (AlwaysShowWhenSelected && State == SelectionState.Selected);

        protected SentakkiSelectionBlueprint(DrawableHitObject drawableObject)
            : base(drawableObject)
        {
        }

        // This is needed because somehow the SSDQ may not be oriented in a sensible way
        // Due to that, the selection boundaries may be negative because the SelectionHandler doesn't make sure that BottomRight > TopLeft
        // I can't say I like this patchwork, as I would very much like to just declare the quad and it just works...
        protected Quad GetCorrectedQuad(Quad originalQuad)
        {
            var topLeft = Vector2.ComponentMin(originalQuad.TopLeft, Vector2.ComponentMin(originalQuad.TopRight, Vector2.ComponentMin(originalQuad.BottomLeft, originalQuad.BottomRight)));
            var bottomRight = Vector2.ComponentMax(originalQuad.TopLeft, Vector2.ComponentMax(originalQuad.TopRight, Vector2.ComponentMax(originalQuad.BottomLeft, originalQuad.BottomRight)));

            return new Quad(topLeft, Vector2.Zero, Vector2.Zero, bottomRight);
        }
    }
}
