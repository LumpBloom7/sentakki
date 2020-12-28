using osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds.Components;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Rulesets.Edit;
using osuTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Framework.Input.Events;
using osuTK.Input;
namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds
{
    public class TouchHoldPlacementBlueprint : PlacementBlueprint
    {
        public new TouchHold HitObject => (TouchHold)base.HitObject;

        private readonly TouchHoldSelectionPiece selectionHighlight;

        public TouchHoldPlacementBlueprint()
            : base(new TouchHold())
        {
            RelativeSizeAxes = Axes.None;
            Size = new Vector2(600);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            InternalChild = selectionHighlight = new TouchHoldSelectionPiece();
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (e.Button != MouseButton.Left)
                return;

            base.OnMouseUp(e);
            if (HitObject.EndTime < HitObject.StartTime)
            {
                var tmp = HitObject.StartTime;
                HitObject.StartTime = HitObject.EndTime;
                HitObject.EndTime = tmp;
            }
            EndPlacement(true);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
                return base.OnMouseDown(e);

            BeginPlacement(true);
            return true;
        }

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            base.UpdateTimeAndPosition(result);
            if (PlacementActive)
                HitObject.EndTime = EditorClock.CurrentTime;
        }
    }
}
