using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touchs.Components;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touchs
{
    public class TouchSelectionBlueprint : SentakkiSelectionBlueprint<Touch>
    {
        protected new DrawableTouch DrawableObject => (DrawableTouch)base.DrawableObject;

        protected readonly TouchSelectionPiece SelectionPiece;

        public TouchSelectionBlueprint(DrawableTouch drawableCircle)
            : base(drawableCircle)
        {
            InternalChild = SelectionPiece = new TouchSelectionPiece();
        }

        protected override void Update()
        {
            base.Update();
            SelectionPiece.UpdateFrom(DrawableObject);
        }

        public override Vector2 ScreenSpaceSelectionPoint => DrawableObject.ScreenSpaceDrawQuad.Centre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => DrawableObject.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => SelectionPiece.ScreenSpaceDrawQuad;
    }
}
