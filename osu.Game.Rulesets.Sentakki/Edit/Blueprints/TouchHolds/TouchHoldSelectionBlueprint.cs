using osu.Framework.Allocation;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Screens.Edit;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds
{
    public partial class TouchHoldSelectionBlueprint : SentakkiSelectionBlueprint<TouchHold>
    {
        public new DrawableTouchHold DrawableObject => (DrawableTouchHold)base.DrawableObject;

        private readonly TouchHoldHighlight highlight;

        public TouchHoldSelectionBlueprint(TouchHold hitObject)
            : base(hitObject)
        {
            InternalChild = highlight = new TouchHoldHighlight();
        }

        [Resolved]
        private EditorClock editorClock { get; set; } = null!;


        protected override void Update()
        {
            base.Update();

            highlight.Size = DrawableObject.TouchHoldBody.Size;
            highlight.CompletedCentre.Alpha = DrawableObject.TouchHoldBody.CompletedCentre.Alpha;
            highlight.CentrePiece.Alpha = DrawableObject.TouchHoldBody.CentrePiece.Alpha;
            highlight.Position = DrawableObject.Position;
            highlight.ProgressPiece.ProgressBindable.Value = DrawableObject.TouchHoldBody.ProgressPiece.ProgressBindable.Value;
        }

        public override Vector2 ScreenSpaceSelectionPoint => highlight.ScreenSpaceDrawQuad.Centre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => highlight.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => highlight.ScreenSpaceDrawQuad;
    }
}
