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

        public override void UpdatePosition(SnapResult result)
        {
            base.UpdatePosition(result);

            Vector2 newPos = ToLocalSpace(result.ScreenSpacePosition);
            newPos = new Vector2(newPos.X - 300, -(newPos.Y - 300));

            float angle = newPos.GetDegreesFromPosition(Vector2.Zero);
            float distance = Math.Clamp(Vector2.Distance(newPos, Vector2.Zero), 0, 200);

            newPos = SentakkiExtensions.GetCircularPosition(distance, angle);

            HitObject.Position = newPos;
        }
    }
}
