using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds.Components;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds
{
    public class HoldSelectionBlueprint : SentakkiSelectionBlueprint<Hold>
    {
        protected new DrawableHold DrawableObject => (DrawableHold)base.DrawableObject;

        protected readonly HoldSelection SelectionPiece;

        public HoldSelectionBlueprint(DrawableHold drawableCircle)
            : base(drawableCircle)
        {
            InternalChild = SelectionPiece = new HoldSelection();
        }

        protected override void Update()
        {
            base.Update();

            SelectionPiece.UpdateFrom(DrawableObject);
        }

        public override Vector2 ScreenSpaceSelectionPoint => DrawableObject.NoteBody.ScreenSpaceDrawQuad.Centre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => DrawableObject.NoteBody.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => SelectionPiece.SelectionBoundaries;
    }
}
