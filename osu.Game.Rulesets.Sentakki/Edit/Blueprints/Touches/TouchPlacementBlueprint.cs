using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touches
{
    public partial class TouchPlacementBlueprint : SentakkiPlacementBlueprint<Touch>
    {
        private readonly TouchHighlight highlight;

        public TouchPlacementBlueprint()
        {
            InternalChild = highlight = new TouchHighlight();
        }

        protected override void Update()
        {
            highlight.Position = HitObject.Position;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            EndPlacement(true);
            return true;
        }

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            base.UpdateTimeAndPosition(result);

            var newPosition = ToLocalSpace(result.ScreenSpacePosition) - OriginPosition;

            if (Vector2.Distance(Vector2.Zero, newPosition) > 250)
            {
                float angle = Vector2.Zero.GetDegreesFromPosition(newPosition);
                newPosition = SentakkiExtensions.GetCircularPosition(250, angle);
            }

            HitObject.Position = newPosition;
        }
    }
}
