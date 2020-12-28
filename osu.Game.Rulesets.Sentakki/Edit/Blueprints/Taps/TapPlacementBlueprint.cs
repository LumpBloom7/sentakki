using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps.Components;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK.Input;
using osuTK;
using osu.Game.Rulesets.Sentakki.UI;
using System;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps
{
    public class TapPlacementBlueprint : PlacementBlueprint
    {
        public new Tap HitObject => (Tap)base.HitObject;

        private readonly TapPiece circlePiece;

        public TapPlacementBlueprint()
            : base(new Tap())
        {
            RelativeSizeAxes = Axes.None;
            Size = new Vector2(600);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            InternalChild = circlePiece = new TapPiece();
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

            HitObject.Lane = Vector2.Zero.GetDegreesFromPosition(newPos).GetNoteLaneFromDegrees();
        }
    }
}
