using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides
{
    public class SlidePlacementBlueprint : PlacementBlueprint
    {
        private readonly SlideStarHighlight highlight;

        public new Slide HitObject => (Slide)base.HitObject;
        public SlidePlacementBlueprint()
            : base(new Slide()
            {
                SlideInfoList = new List<SentakkiSlideInfo>{
                    new SentakkiSlideInfo{
                        ID = 13,
                        Duration = 1000,
                    }
                }
            })
        {
            Anchor = Origin = Anchor.Centre;
            InternalChild = highlight = new SlideStarHighlight();
            highlight.Note.Y = -SentakkiPlayfield.INTERSECTDISTANCE;
        }

        protected override void Update()
        {
            highlight.Rotation = HitObject.Lane.GetRotationForLane();
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

            HitObject.Lane = OriginPosition.GetDegreesFromPosition(ToLocalSpace(result.ScreenSpacePosition)).GetNoteLaneFromDegrees();
        }
    }
}
