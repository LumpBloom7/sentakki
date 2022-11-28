using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides
{
    public class SlidePlacementBlueprint : SentakkiPlacementBlueprint<Slide>
    {
        private readonly SlideTapHighlight highlight;

        private readonly SlideVisual bodyHighlight;
        private readonly SlideVisual commited;

        [Resolved]
        private IBeatSnapProvider beatSnapProvider { get; set; } = null!;

        private OsuSpriteText shootDelayText;

        public SlidePlacementBlueprint()
        {
            AddRangeInternal(new Drawable[]
            {
                highlight = new SlideTapHighlight(),
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Rotation = -22.5f,
                    Children = new Drawable[]
                    {
                        commited = new SlideVisual()
                        {
                            Colour = Color4.LimeGreen,
                            Alpha = 0.5f,
                        },
                        bodyHighlight = new SlideVisual()
                        {
                            Colour = Color4.GreenYellow,
                            Alpha = 0.8f,
                        },
                    }
                },
                shootDelayText = new OsuSpriteText(){
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "Shoot Delay: 1 beat",
                    Font = OsuFont.Torus.With(size: 30f),
                    Alpha = 0,
                }
            });

            highlight.SlideTapPiece.Y = -SentakkiPlayfield.INTERSECTDISTANCE;
            highlight.SlideTapPiece.Scale = Vector2.One;
        }

        private SlideBodyInfo commitedSlideBodyInfo = null!;
        private SlideBodyInfo previewSlideBodyInfo = null!;
        private int currentLaneOffset;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            if (PlacementActive != PlacementState.Active)
            {
                BeginPlacement(true);

                HitObject.SlideInfoList.Add(commitedSlideBodyInfo = new SlideBodyInfo());

                commited.Rotation = HitObject.Lane.GetRotationForLane();
                bodyHighlight.Rotation = HitObject.Lane.GetRotationForLane();

                previewSlideBodyInfo = new SlideBodyInfo
                {
                    SlidePathParts = new[] { currentPart = new SlideBodyPart(SlidePaths.PathShapes.Straight, 4, false) }
                };
                bodyHighlight.Path = previewSlideBodyInfo.SlidePath;

                shootDelayText.Position = SentakkiExtensions.GetCircularPosition(330, HitObject.Lane.GetRotationForLane());
                shootDelayText.Alpha = 1;
            }
            else
            {
                commitCurrentPart();
            }

            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (e.Button != MouseButton.Right)
                return;

            if (PlacementActive == PlacementState.Active)
                EndPlacement(bodyParts.Count > 0 && commitedSlideBodyInfo.Duration > 0);
        }

        private void updateShootDelayText()
        {
            shootDelayText.Text = string.Format("Shoot delay: {0} beats", commitedSlideBodyInfo.ShootDelay);
            shootDelayText.ScaleTo(1.1f, 20).Then().ScaleTo(1f, 30);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (PlacementActive != PlacementState.Active)
                return false;

            switch (e.Key)
            {
                case Key.Plus:
                    commitedSlideBodyInfo.ShootDelay += 1f / beatSnapProvider.BeatDivisor;
                    updateShootDelayText();
                    break;

                case Key.Minus:
                    commitedSlideBodyInfo.ShootDelay = Math.Max(0, commitedSlideBodyInfo.ShootDelay - 1f / beatSnapProvider.BeatDivisor);
                    updateShootDelayText();
                    break;

                case Key.Number0:
                    commitedSlideBodyInfo.ShootDelay = 1;
                    updateShootDelayText();
                    break;

                case Key.BackSpace:
                    uncommitLastPart();
                    break;

                case Key.BackSlash:
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

        private readonly List<SlideBodyPart> bodyParts = new List<SlideBodyPart>();

        // Only used to revert the lane offsets after uncommit
        private readonly Stack<int> laneOffsets = new Stack<int>();

        private SlideBodyPart currentPart = null!;

        private SlidePaths.PathShapes currentShape = SlidePaths.PathShapes.Straight;

        // The pathOffset of a valid path that matches or closely matches the desired endOffset
        // This will be used when committing a part
        private int validPathOffset;

        // The path offset of the lane the player is pointing at
        private int targetPathOffset;
        private bool mirrored;

        private double originalStartTime;

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            base.UpdateTimeAndPosition(result);

            if (PlacementActive == PlacementState.Active)
            {
                if (result.Time is double endTime)
                {
                    HitObject.StartTime = endTime < originalStartTime ? endTime : originalStartTime;
                    commitedSlideBodyInfo.Duration = Math.Abs(endTime - originalStartTime);
                }

                int newPO = (OriginPosition.GetDegreesFromPosition(ToLocalSpace(result.ScreenSpacePosition)).GetNoteLaneFromDegrees() - currentLaneOffset - HitObject.Lane).NormalizePath();

                if (targetPathOffset != newPO)
                    updateCurrentPathPart(newPO);
            }
            else
            {
                HitObject.Lane = OriginPosition.GetDegreesFromPosition(ToLocalSpace(result.ScreenSpacePosition)).GetNoteLaneFromDegrees();
                highlight.Rotation = HitObject.Lane.GetRotationForLane();
                if (result.Time is double startTime)
                    originalStartTime = HitObject.StartTime = startTime;
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

        private void commitCurrentPart()
        {
            laneOffsets.Push(validPathOffset);
            bodyParts.Add(currentPart);

            currentLaneOffset += validPathOffset;
            commitedSlideBodyInfo.SlidePathParts = bodyParts.ToArray();
            commited.Path = commitedSlideBodyInfo.SlidePath;

            bodyHighlight.Rotation = (HitObject.Lane + currentLaneOffset).GetRotationForLane();
        }

        private void uncommitLastPart()
        {
            if (laneOffsets.Count == 0)
                return;

            currentLaneOffset -= laneOffsets.Pop();
            bodyParts.RemoveAt(bodyParts.Count - 1);

            commitedSlideBodyInfo.SlidePathParts = bodyParts.ToArray();
            commited.Path = commitedSlideBodyInfo.SlidePath;

            bodyHighlight.Rotation = (HitObject.Lane + currentLaneOffset).GetRotationForLane();
        }
    }
}
