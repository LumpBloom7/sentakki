using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps
{
    public partial class TapPlacementBlueprint : SentakkiPlacementBlueprint<Tap>
    {
        private readonly TapHighlight highlight;

        public TapPlacementBlueprint()
        {
            InternalChild = highlight = new TapHighlight();
            highlight.Note.Y = -SentakkiPlayfield.INTERSECTDISTANCE;
        }

        protected override void Update()
        {
            highlight.Rotation = HitObject.Lane.GetRotationForLane();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left) return false;

            EndPlacement(true);
            return true;
        }

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            base.UpdateTimeAndPosition(result);

            HitObject.Lane = OriginPosition.GetDegreesFromPosition(ToLocalSpace(result.ScreenSpacePosition)).GetNoteLaneFromDegrees();
        }
    }
}
