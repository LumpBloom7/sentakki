using System;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds
{
    public partial class TouchHoldPlacementBlueprint : SentakkiPlacementBlueprint<TouchHold>
    {
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

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            base.UpdateTimeAndPosition(result);

            if (PlacementActive == PlacementState.Active)
            {
                if (result.Time is double endTime)
                {
                    HitObject.StartTime = endTime < originalStartTime ? endTime : originalStartTime;
                    HitObject.Duration = Math.Abs(endTime - originalStartTime);
                }
            }
            else
            {
                if (result.Time is double startTime)
                    originalStartTime = HitObject.StartTime = startTime;
            }
        }
    }
}
