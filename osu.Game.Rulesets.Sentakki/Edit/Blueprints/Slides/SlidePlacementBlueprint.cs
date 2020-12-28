using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides.Components;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Rulesets.Edit;
using osuTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Framework.Input.Events;
using osuTK.Input;
using System.Linq;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides
{
    public class SlidePlacementBlueprint : PlacementBlueprint
    {
        public new Slide HitObject => (Slide)base.HitObject;

        private readonly SlideSelection selectionHighlight;

        public SlidePlacementBlueprint()
            : base(new Slide())
        {
            RelativeSizeAxes = Axes.None;
            Size = new Vector2(600);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            InternalChild = selectionHighlight = new SlideSelection();
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
            if (HitObject.EndTime <= HitObject.StartTime)
            {
                EndPlacement(false);
                return;
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

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            base.UpdateTimeAndPosition(result);
            if (!PlacementActive)
            {
                Vector2 newPos = ToLocalSpace(result.ScreenSpacePosition) - new Vector2(300);

                HitObject.Lane = Vector2.Zero.GetDegreesFromPosition(newPos).GetNoteLaneFromDegrees();
            }
            else
            {
                HitObject.SlideInfoList.First().Duration = EditorClock.CurrentTime - HitObject.StartTime;
            }
        }
    }
}
