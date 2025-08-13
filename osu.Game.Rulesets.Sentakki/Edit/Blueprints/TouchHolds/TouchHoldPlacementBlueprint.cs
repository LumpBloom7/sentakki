using System;
using osu.Framework.Allocation;
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

        [Resolved]
        private SentakkiHitObjectComposer composer { get; set; } = null!;

        private readonly TouchHoldHighlight highlight;

        public TouchHoldPlacementBlueprint()
        {
            InternalChild = highlight = new TouchHoldHighlight();
        }

        protected override void Update()
        {
            highlight.Position = HitObject.Position;
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
            var result = composer?.FindSnappedPositionAndTime(screenSpacePosition) ?? new SnapResult(screenSpacePosition, fallbackTime);

            base.UpdateTimeAndPosition(result.ScreenSpacePosition, result.Time ?? fallbackTime);

            if (PlacementActive == PlacementState.Active)
            {
                HitObject.StartTime = fallbackTime < originalStartTime ? fallbackTime : originalStartTime;
                HitObject.Duration = Math.Abs(fallbackTime - originalStartTime);

                if (HitObject.Duration > 0)
                    timeChanged = true;
            }
            else
            {
                originalStartTime = HitObject.StartTime = result.Time ?? fallbackTime;

                var newPosition = ToLocalSpace(result.ScreenSpacePosition) - OriginPosition;

                if (Vector2.Distance(Vector2.Zero, newPosition) > 270)
                {
                    float angle = Vector2.Zero.GetDegreesFromPosition(newPosition);
                    newPosition = SentakkiExtensions.GetCircularPosition(270, angle);
                }

                HitObject.Position = newPosition;
            }

            return result;
        }
    }
}
