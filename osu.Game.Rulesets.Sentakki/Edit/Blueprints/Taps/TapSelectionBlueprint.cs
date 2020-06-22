using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps.Components;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps
{
    public class TapSelectionBlueprint : SentakkiSelectionBlueprint<Tap>
    {
        protected new DrawableTap DrawableObject => (DrawableTap)base.DrawableObject;

        protected readonly TapPiece CirclePiece;

        public TapSelectionBlueprint(DrawableTap drawableCircle)
            : base(drawableCircle)
        {
            InternalChild = CirclePiece = new TapPiece();
        }

        protected override void Update()
        {
            base.Update();

            CirclePiece.UpdateFrom(HitObject);
        }

        public override Vector2 ScreenSpaceSelectionPoint => DrawableObject.HitArea.ScreenSpaceDrawQuad.Centre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => DrawableObject.HitArea.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => DrawableObject.HitArea.ScreenSpaceDrawQuad;
    }
}