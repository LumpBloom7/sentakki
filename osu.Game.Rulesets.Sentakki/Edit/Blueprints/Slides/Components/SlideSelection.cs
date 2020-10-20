using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides.Components
{
    public class SlideSelection : BlueprintPiece<Slide>
    {
        // This needs to be shrunk because the AABB box has larger margins for some reason
        //public Quad SelectionBoundaries => notebody.ScreenSpaceDrawQuad.AABBFloat.Shrink(10f);

        private Drawable starPiece;

        public SlideSelection()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(80);
            AddInternal(starPiece = new StarPiece());
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
        }

        public override void UpdateFrom(Slide hitObject)
        {
            base.UpdateFrom(hitObject);
            Rotation = hitObject.Lane.GetRotationForLane();
        }

        public void UpdateFrom(DrawableSlide drawableSlide)
        {
            var lho = drawableSlide.HitObject as SentakkiLanedHitObject;
            Rotation = lho.Lane.GetRotationForLane();
            starPiece.Rotation = ((SlideTapPiece)drawableSlide.SlideTaps.Child.TapVisual).Stars.Rotation;
            Position = SentakkiExtensions.GetCircularPosition(-drawableSlide.SlideTaps.Child.TapVisual.Position.Y, lho.Lane.GetRotationForLane());
        }
    }
}
