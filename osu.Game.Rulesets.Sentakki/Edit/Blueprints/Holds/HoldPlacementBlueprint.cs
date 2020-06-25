using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds.Components;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Rulesets.Edit;
using osuTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Framework.Input.Events;
using osuTK.Input;
namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds
{
    public class HoldPlacementBlueprint : PlacementBlueprint
    {
        public new Hold HitObject => (Hold)base.HitObject;

        private readonly HoldSelection selectionHighlight;

        public HoldPlacementBlueprint()
            : base(new Hold())
        {
            RelativeSizeAxes = Axes.None;
            Size = new Vector2(600);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            InternalChild = selectionHighlight = new HoldSelection();
        }

        protected override void Update()
        {
            base.Update();

            selectionHighlight.UpdateFrom(HitObject);
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

        private double firstTime = 0;

        public override void UpdatePosition(SnapResult result)
        {
            base.UpdatePosition(result);
            if (!PlacementActive)
            {
                Vector2 newPos = ToLocalSpace(result.ScreenSpacePosition);
                newPos.Y = 600 - newPos.Y;
                newPos -= new Vector2(300);

                var path = newPos.GetDegreesFromPosition(Vector2.Zero).GetNotePathFromDegrees();
                var angle = path.GetAngleFromPath();

                HitObject.Angle = angle;

                HitObject.EndPosition = new Vector2(0, -SentakkiPlayfield.INTERSECTDISTANCE + 40);

                HitObject.StartTime = EditorClock.CurrentTimeAccurate;
                firstTime = EditorClock.CurrentTimeAccurate;
            }
            else
            {
                HitObject.EndTime = EditorClock.CurrentTimeAccurate;
            }
        }
    }
}