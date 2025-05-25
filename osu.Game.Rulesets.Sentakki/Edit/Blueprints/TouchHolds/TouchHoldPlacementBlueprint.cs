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
            timeChanged = false;
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (PlacementActive == PlacementState.Active)
                if ((e.Button is MouseButton.Left && timeChanged) || e.Button is MouseButton.Right)
                    EndPlacement(HitObject.Duration > 0);
        }

        private double originalStartTime;
        private bool timeChanged = false;

        public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double fallbackTime)
        {
            if (PlacementActive == PlacementState.Active)
            {
                HitObject.StartTime = fallbackTime < originalStartTime ? fallbackTime : originalStartTime;
                HitObject.Duration = Math.Abs(fallbackTime - originalStartTime);

                if (HitObject.Duration > 0)
                    timeChanged = true;
            }
            else
            {
                originalStartTime = HitObject.StartTime = fallbackTime;
            }

            return new SnapResult(screenSpacePosition, fallbackTime);
        }
    }
}
