using System;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds
{
    public partial class TouchHoldPlacementBlueprint : SentakkiPlacementBlueprint<TouchHold>
    {
        protected override bool IsValidForPlacement => HitObject.Duration > 0;

        public TouchHoldPlacementBlueprint()
        {
            InternalChild = new TouchHoldHighlight();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            if (PlacementActive == PlacementState.Active)
                return false;

            BeginPlacement(true);
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (e.Button != MouseButton.Right)
                return;

            if (PlacementActive == PlacementState.Active)
                EndPlacement(HitObject.Duration > 0);
        }

        private double originalStartTime;

        public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double fallbackTime)
        {
            if (PlacementActive == PlacementState.Active)
            {
                if (EditorClock.CurrentTime is double endTime)
                {
                    HitObject.StartTime = endTime < originalStartTime ? endTime : originalStartTime;
                    HitObject.Duration = Math.Abs(endTime - originalStartTime);
                }
            }
            else
            {
                if (EditorClock.CurrentTime is double startTime)
                    originalStartTime = HitObject.StartTime = startTime;
            }

            return new SnapResult(screenSpacePosition, fallbackTime);
        }
    }
}
