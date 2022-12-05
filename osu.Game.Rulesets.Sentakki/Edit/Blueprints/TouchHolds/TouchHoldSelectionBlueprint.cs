using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
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

        protected override void Update()
        {
            base.Update();

            highlight.Position = DrawableObject.Position;
            highlight.Scale = DrawableObject.Scale;
            highlight.ProgressPiece.ProgressBindable.Value = DrawableObject.TouchHoldBody.ProgressPiece.ProgressBindable.Value;
        }

        public override Vector2 ScreenSpaceSelectionPoint => highlight.ScreenSpaceDrawQuad.Centre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => highlight.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => highlight.ScreenSpaceDrawQuad;
    }
}
