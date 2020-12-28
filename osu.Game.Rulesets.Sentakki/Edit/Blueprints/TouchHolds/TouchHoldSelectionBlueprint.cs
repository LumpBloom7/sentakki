using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds.Components;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds
{
    public class TouchHoldSelectionBlueprint : SentakkiSelectionBlueprint<TouchHold>
    {
        protected new DrawableTouchHold DrawableObject => (DrawableTouchHold)base.DrawableObject;

        protected readonly TouchHoldSelectionPiece SelectionPiece;

        public TouchHoldSelectionBlueprint(DrawableTouchHold drawableCircle)
            : base(drawableCircle)
        {
            InternalChild = SelectionPiece = new TouchHoldSelectionPiece();
        }

        protected override void Update()
        {
            base.Update();
        }

        public override Vector2 ScreenSpaceSelectionPoint => DrawableObject.ScreenSpaceDrawQuad.Centre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => DrawableObject.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => SelectionPiece.SelectionBoundaries;
    }
}
