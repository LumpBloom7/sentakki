using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides.Components;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides
{
    public class SlidesSelectionBlueprint : SentakkiSelectionBlueprint<Slide>
    {
        protected new DrawableSlide DrawableObject => (DrawableSlide)base.DrawableObject;

        protected readonly SlideSelection SelectionPiece;

        public SlidesSelectionBlueprint(DrawableSlide drawableCircle)
            : base(drawableCircle)
        {
            InternalChild = SelectionPiece = new SlideSelection();
        }

        protected override void Update()
        {
            base.Update();

            SelectionPiece.UpdateFrom(DrawableObject);
        }

        public override Vector2 ScreenSpaceSelectionPoint => DrawableObject.SlideTaps.Child.TapVisual.ScreenSpaceDrawQuad.Centre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => DrawableObject.SlideTaps.Child.TapVisual.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => DrawableObject.SlideTaps.Child.TapVisual.ScreenSpaceDrawQuad.AABB;
    }
}
