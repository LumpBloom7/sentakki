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
        private readonly SlideVisual commited = null!;

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
                    Children = new Drawable[]{
                        commited = new SlideVisual(){
                            Colour = Color4.LimeGreen,
                            Alpha = 0.5f,
                        },
                        bodyHighlight = new SlideVisual(){
                            Colour = Color4.GreenYellow,
                            Alpha = 0.8f,
                        },
                    }
                }
            });

            highlight.SlideTapPiece.Y = -SentakkiPlayfield.INTERSECTDISTANCE;
            highlight.SlideTapPiece.Scale = Vector2.One;
        }


        private SlideBodyInfo commitedSlideBodyInfo = null!;
        private SlideBodyInfo previewSlideBodyInfo = null!;
        private int currentLaneOffset;

        protected override bool OnClick(ClickEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            if (PlacementActive != PlacementState.Active)
            {
                BeginPlacement(true);

                HitObject.SlideInfoList.Add(commitedSlideBodyInfo = new SlideBodyInfo());
                previewSlideBodyInfo = new SlideBodyInfo();

                commited.Rotation = HitObject.Lane.GetRotationForLane();
                bodyHighlight.Rotation = HitObject.Lane.GetRotationForLane();
            }
            else
            {
                currentLaneOffset += validPathOffset;
                bodyParts.Add(currentPart);
                commitedSlideBodyInfo.SlidePathParts = bodyParts.ToArray();
                commited.Path = commitedSlideBodyInfo.SlidePath;


                bodyHighlight.Rotation = (HitObject.Lane + currentLaneOffset).GetRotationForLane();

                targetPathOffset = -999; // Force re-evaluation of current path part
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
            if (PlacementActive != PlacementState.Active)
                return false;

            switch (e.Key)
            {
                case Key.M:
                    mirrored = !mirrored;
                    updateCurrentPathPart(targetPathOffset);
                    return true;

                case Key.BracketRight:
                    currentShape = (SlidePaths.PathShapes)((int)(currentShape + 1) % 8);
                    updateCurrentPathPart(targetPathOffset);
                    return true;

                case Key.BracketLeft:
                    currentShape = (SlidePaths.PathShapes)((int)(currentShape + 7) % 8);
                    updateCurrentPathPart(targetPathOffset);
                    return true;
            }

            return base.OnKeyDown(e);
        }

        private List<SlideBodyPart> bodyParts = new List<SlideBodyPart>();

        private SlideBodyPart currentPart = null!;

        private SlidePaths.PathShapes currentShape = SlidePaths.PathShapes.Straight;

        // The pathOffset of a valid path that matches or closely matches the desired endOffset
        // This will be used when committing a part
        private int validPathOffset = 0;

        // The path offset of the lane the player is pointing at
        private int targetPathOffset = 0;
        private bool mirrored = false;

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            base.UpdateTimeAndPosition(result);

            if (PlacementActive == PlacementState.Active)
            {
                if (result.Time is double endTime)
                    commitedSlideBodyInfo.Duration = endTime - HitObject.StartTime;

                int newPO = (OriginPosition.GetDegreesFromPosition(ToLocalSpace(result.ScreenSpacePosition)).GetNoteLaneFromDegrees() - currentLaneOffset - HitObject.Lane).NormalizePath();

                if (targetPathOffset != newPO)
                    updateCurrentPathPart(newPO);
            }
            else
            {
                HitObject.Lane = OriginPosition.GetDegreesFromPosition(ToLocalSpace(result.ScreenSpacePosition)).GetNoteLaneFromDegrees();
                highlight.Rotation = HitObject.Lane.GetRotationForLane();
                if (result.Time is double startTime)
                    HitObject.StartTime = startTime;
            }
        }

        private void updateCurrentPathPart(int newTargetOffset)
        {
            int oldValidPath = validPathOffset;
            validPathOffset = newTargetOffset;

            var newPart = new SlideBodyPart(currentShape, newTargetOffset, mirrored);

            if (!SlidePaths.CheckSlideValidity(newPart))
            {
                bool tryNegativeDeltaFirst = (oldValidPath - newTargetOffset) < 0;

                // Find closest valid offset
                for (int i = 1; i < 8; ++i)
                {
                    bool negative = ((i % 2) == 0) ^ tryNegativeDeltaFirst;

                    int delta = (i + 1) / 2 * (negative ? -1 : 1);

                    int candidateOffset = (newTargetOffset + delta).NormalizePath();

                    newPart = new SlideBodyPart(currentShape, candidateOffset, mirrored);

                    if (SlidePaths.CheckSlideValidity(newPart))
                    {
                        validPathOffset = candidateOffset;
                        break;
                    }
                }
            }

            currentPart = newPart;
            previewSlideBodyInfo.SlidePathParts = new[] { currentPart };
            bodyHighlight.Path = previewSlideBodyInfo.SlidePath;
            targetPathOffset = newTargetOffset;
        }
    }
}
