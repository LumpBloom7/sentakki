using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides
{
    public partial class SlidePlacementBlueprint : SentakkiPlacementBlueprint<Slide>
    {
        private readonly SlideTapHighlight highlight;

        private readonly SlideVisual bodyHighlight;
        private readonly SlideVisual commited;

        [Resolved]
        private SlideEditorToolboxGroup slidePlacementToolbox { get; set; } = null!;

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
                        commited = new SlideVisual
                        {
                            Colour = Color4.LimeGreen,
                            Alpha = 0.5f,
                        },
                        bodyHighlight = new SlideVisual
                        {
                            Colour = Color4.GreenYellow,
                            Alpha = 0f,
                        },
                    }
                },
            });

            highlight.SlideTapPiece.Y = -SentakkiPlayfield.INTERSECTDISTANCE;
            highlight.SlideTapPiece.Scale = Vector2.One;
        }

        [Resolved]
        private SentakkiSnapProvider snapProvider { get; set; } = null!;

        protected override void Update()
        {
            base.Update();
            highlight.Rotation = HitObject.Lane.GetRotationForLane();
            highlight.SlideTapPiece.Y = -snapProvider.GetDistanceRelativeToCurrentTime(HitObject.StartTime, SentakkiPlayfield.NOTESTARTDISTANCE);
        }

        private SlideBodyInfo? commitedSlideBodyInfo = null!;
        private SlideBodyInfo previewSlideBodyInfo = null!;
        private int currentLaneOffset;

        private Bindable<SlideBodyPart> currentPart = new Bindable<SlideBodyPart>();
        private Bindable<float> shootDelay = new Bindable<float>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentPart.BindTo(slidePlacementToolbox.CurrentPartBindable);
            currentPart.BindValueChanged(v =>
            {
                previewSlideBodyInfo = new SlideBodyInfo
                {
                    SlidePathParts = new[] { v.NewValue }
                };
                bodyHighlight.Path = previewSlideBodyInfo.SlidePath;
            }, true);

            shootDelay.BindTo(slidePlacementToolbox.ShootDelayBindable);
            shootDelay.BindValueChanged(v =>
            {
                if (commitedSlideBodyInfo is not null)
                    commitedSlideBodyInfo.ShootDelay = v.NewValue;
            });
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            if (PlacementActive != PlacementState.Active)
            {
                BeginPlacement(true);

                EditorClock.SeekSmoothlyTo(HitObject.StartTime);

                HitObject.SlideInfoList.Add(commitedSlideBodyInfo = new SlideBodyInfo()
                {
                    ShootDelay = shootDelay.Value
                });

                commited.Rotation = HitObject.Lane.GetRotationForLane();
                bodyHighlight.Rotation = HitObject.Lane.GetRotationForLane();
                bodyHighlight.Alpha = 0.8f;
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
            {
                Debug.Assert(commitedSlideBodyInfo is not null);
                EndPlacement(bodyParts.Count > 0 && commitedSlideBodyInfo.Duration > 0);
            }
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (PlacementActive != PlacementState.Active)
                return base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.BackSpace:
                    uncommitLastPart();
                    break;
            }

            return base.OnKeyDown(e);
        }

        private readonly List<SlideBodyPart> bodyParts = new List<SlideBodyPart>();

        // Only used to revert the lane offsets after uncommit
        private readonly Stack<int> laneOffsets = new Stack<int>();

        // The path offset of the lane the player is pointing at
        private int targetPathOffset;

        private double originalStartTime;

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            if (result is not SentakkiLanedSnapResult senRes)
                return;

            if (PlacementActive == PlacementState.Active)
            {
                Debug.Assert(commitedSlideBodyInfo is not null);
                double endTime = EditorClock.CurrentTime;

                HitObject.StartTime = endTime < originalStartTime ? endTime : originalStartTime;
                commitedSlideBodyInfo.Duration = Math.Abs(endTime - originalStartTime);

                var localSpacePointerCoord = ToLocalSpace(result.ScreenSpacePosition);

                if ((localSpacePointerCoord - OriginPosition).LengthSquared > 400 * 400)
                    return;

                int newPo = (senRes.Lane - currentLaneOffset - HitObject.Lane).NormalizePath();

                if (targetPathOffset != newPo)
                {
                    slidePlacementToolbox.RequestLaneChange(newPo, true);
                    targetPathOffset = newPo;
                }
            }
            else
            {
                base.UpdateTimeAndPosition(result);

                HitObject.Lane = senRes.Lane;

                if (result.Time is double startTime)
                    originalStartTime = HitObject.StartTime = startTime;
            }
        }

        private void commitCurrentPart()
        {
            Debug.Assert(commitedSlideBodyInfo is not null);
            laneOffsets.Push(slidePlacementToolbox.CurrentPart.EndOffset);
            bodyParts.Add(slidePlacementToolbox.CurrentPart);

            currentLaneOffset += slidePlacementToolbox.CurrentPart.EndOffset;
            commitedSlideBodyInfo.SlidePathParts = bodyParts.ToArray();
            commited.Path = commitedSlideBodyInfo.SlidePath;

            bodyHighlight.Rotation = (HitObject.Lane + currentLaneOffset).GetRotationForLane();
        }

        private void uncommitLastPart()
        {
            Debug.Assert(commitedSlideBodyInfo is not null);
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
