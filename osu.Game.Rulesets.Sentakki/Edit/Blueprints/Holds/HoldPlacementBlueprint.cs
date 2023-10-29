using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Screens.Edit;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds
{
    public partial class HoldPlacementBlueprint : SentakkiPlacementBlueprint<Hold>
    {
        private readonly HoldHighlight highlight;

        public HoldPlacementBlueprint()
        {
            InternalChild = highlight = new HoldHighlight();
            highlight.Note.Y = -SentakkiPlayfield.INTERSECTDISTANCE;
        }

        [Resolved]
        private SentakkiSnapGrid snapGrid { get; set; } = null!;

        protected override void Update()
        {
            highlight.Rotation = HitObject.Lane.GetRotationForLane();
            highlight.Note.Y = -snapGrid.GetDistanceRelativeToCurrentTime(HitObject.StartTime, SentakkiPlayfield.NOTESTARTDISTANCE);
            highlight.Note.Height = -snapGrid.GetDistanceRelativeToCurrentTime(HitObject.EndTime, SentakkiPlayfield.NOTESTARTDISTANCE) - highlight.Note.Y;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            if (PlacementActive == PlacementState.Active)
                return false;

            BeginPlacement(true);
            EditorClock.SeekSmoothlyTo(HitObject.StartTime);

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
                HitObject.Lane = ((SentakkiSnapResult)result).Lane;
                if (result.Time is double startTime)
                    originalStartTime = HitObject.StartTime = startTime;
            }
        }
    }
}
