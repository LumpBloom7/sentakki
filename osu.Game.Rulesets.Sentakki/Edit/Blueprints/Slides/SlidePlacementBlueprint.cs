using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides
{
    public class SlidePlacementBlueprint : PlacementBlueprint
    {
        private readonly SlideTapHighlight highlight = null!;

        private readonly SlideVisual bodyHighlight = null!;

        public new Slide HitObject => (Slide)base.HitObject;

        public SlidePlacementBlueprint()
            : base(new Slide())
        {
            Anchor = Origin = Anchor.Centre;
            AddRangeInternal(new Drawable[]{
                highlight = new SlideTapHighlight(),
                new Container{
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Rotation = -22.5f,
                    Child = bodyHighlight = new SlideVisual(){
                        Colour = Color4.GreenYellow,
                        Alpha = 0.5f,
                    }
                }
            });
            highlight.SlideTapPiece.Y = -SentakkiPlayfield.INTERSECTDISTANCE;
            highlight.SlideTapPiece.Scale = Vector2.One;
        }

        protected override void Update()
        {
            highlight.Rotation = HitObject.Lane.GetRotationForLane();
            bodyHighlight.Rotation = HitObject.Lane.GetRotationForLane();
        }

        private SlideBodyInfo slideBodyInfo = null!;
        private SlideBodyInfo previewSlideBodyInfo = null!;
        private int currentLaneOffset;

        protected override bool OnClick(ClickEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            if (PlacementActive != PlacementState.Active)
            {
                BeginPlacement(true);
                currentLaneOffset = HitObject.Lane;
                HitObject.SlideInfoList.Add(slideBodyInfo = new SlideBodyInfo());
                previewSlideBodyInfo = new SlideBodyInfo();
                currentLaneOffset = 0;
                index = 0;
            }
            else
            {
                ++index;
                currentLaneOffset += pathOffset;
                slideBodyInfo.SlidePathParts = previewSlideBodyInfo.SlidePathParts;
                bodyHighlight.Path = slideBodyInfo.SlidePath;
            }

            return true;
        }

        protected override bool OnDoubleClick(DoubleClickEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            if (PlacementActive == PlacementState.Active)
            {
                EndPlacement(true);
                return true;
            }

            return base.OnDoubleClick(e);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.M:
                    mirrored = !mirrored;
                    updateCurrentPathPart();
                    return true;

                case Key.BracketRight:
                    currentShape = (SlidePaths.PathShapes)((int)(currentShape + 1) % 8);
                    updateCurrentPathPart();
                    return true;

                case Key.BracketLeft:
                    currentShape = (SlidePaths.PathShapes)((int)(currentShape + 7) % 8);
                    updateCurrentPathPart();
                    return true;
            }

            return base.OnKeyDown(e);
        }

        private SlidePaths.PathShapes currentShape = SlidePaths.PathShapes.Straight;

        private int pathOffset = 0;

        private List<SlideBodyPart> bodyParts = new List<SlideBodyPart>();
        private int index = 0;

        private bool mirrored = false;

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            base.UpdateTimeAndPosition(result);

            if (PlacementActive == PlacementState.Active)
            {
                if (result.Time is double endTime)
                    slideBodyInfo.Duration = endTime - HitObject.StartTime;

                int newPO = (OriginPosition.GetDegreesFromPosition(ToLocalSpace(result.ScreenSpacePosition)).GetNoteLaneFromDegrees() - currentLaneOffset - HitObject.Lane).NormalizePath();

                if (pathOffset != newPO)
                {
                    pathOffset = newPO;
                    updateCurrentPathPart();
                }
            }
            else
            {
                HitObject.Lane = OriginPosition.GetDegreesFromPosition(ToLocalSpace(result.ScreenSpacePosition)).GetNoteLaneFromDegrees();
                if (result.Time is double startTime)
                    HitObject.StartTime = startTime;
            }
        }

        private void updateCurrentPathPart()
        {
            if (bodyParts.Count > index)
                bodyParts.RemoveAt(index);

            bodyParts.Add(new SlideBodyPart(currentShape, pathOffset, mirrored));
            previewSlideBodyInfo.SlidePathParts = bodyParts.ToArray();
            bodyHighlight.Path = previewSlideBodyInfo.SlidePath;
        }
    }
}
