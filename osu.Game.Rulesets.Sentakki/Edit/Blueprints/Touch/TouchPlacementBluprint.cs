using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touchs.Components;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK.Input;
using osuTK;
using osu.Game.Rulesets.Sentakki.UI;
using System;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touchs
{
    public class TouchPlacementBlueprint : PlacementBlueprint
    {
        public new Touch HitObject => (Touch)base.HitObject;

        private readonly TouchSelectionPiece circlePiece;

        public TouchPlacementBlueprint()
            : base(new Touch())
        {
            RelativeSizeAxes = Axes.None;
            Size = new Vector2(600);
            InternalChild = circlePiece = new TouchSelectionPiece();
        }

        protected override void Update()
        {
            base.Update();

            circlePiece.UpdateFrom(HitObject);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Left)
            {
                EndPlacement(true);
                return true;
            }

            return base.OnMouseDown(e);
        }

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            base.UpdateTimeAndPosition(result);

            Vector2 newPos = ToLocalSpace(result.ScreenSpacePosition) - new Vector2(300);

            float angle = Vector2.Zero.GetDegreesFromPosition(newPos);
            float distance = Math.Clamp(Vector2.Distance(newPos, Vector2.Zero), 0, 200);

            newPos = SentakkiExtensions.GetCircularPosition(distance, angle);

            HitObject.Position = newPos;
        }
    }
}
