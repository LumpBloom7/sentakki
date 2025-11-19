using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;

public partial class SlidePlacementBlueprint : SentakkiPlacementBlueprint<Slide>
{
    private readonly SlideTapPiece tapHighlight;

    protected override bool IsValidForPlacement => base.IsValidForPlacement && HitObject.SlideInfoList[0].Segments.Count > 0;

    public SlidePlacementBlueprint()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        HitObject.SlideInfoList = [new SlideBodyInfo()];

        AddInternal(new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Colour = Color4.YellowGreen,
            Children =
            [
                new SlideVisual
                {
                    Rotation = -22.5f,
                    SlideBodyInfo = currentBody,
                },
                tapHighlight = new SlideTapPiece
                {
                    Alpha = 0.5f,

                    Scale = Vector2.One,
                    SecondStar = { Alpha = 0 }
                }
            ]
        });
    }

    protected override void Update()
    {
        base.Update();

        InternalChild.Rotation = HitObject.Lane.GetRotationForLane();
        tapHighlight.Y = -SentakkiPlayfield.INTERSECTDISTANCE;
    }

    private double commitStartTime;

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        switch (PlacementActive)
        {
            case PlacementState.Waiting:
                if (e.Button is not MouseButton.Left)
                    break;

                BeginPlacement(true);
                updateSlideVisual();
                commitStartTime = HitObject.StartTime;
                return true;

            case PlacementState.Active:
                switch (e.Button)
                {
                    case MouseButton.Left:
                        segments.Add(segments[^1]);

                        commitChange();
                        updateSlideVisual();
                        return true;

                    case MouseButton.Middle:
                        if (HitObject.SlideInfoList[0].Segments.Count == 0)
                            break;

                        segments.RemoveAt(segments.Count - 1);

                        commitChange();
                        updateSlideVisual();
                        return true;

                    case MouseButton.Right:
                        EndPlacement(true);
                        return true;
                }

                break;
        }

        return base.OnMouseDown(e);
    }

    private readonly SlideBodyInfo currentBody = new SlideBodyInfo();

    private readonly List<SlideSegment> segments = [new SlideSegment(PathShape.Straight, 4, false)];

    private void updateSlideVisual()
    {
        currentBody.Segments = segments;
    }

    private void commitChange()
    {
        HitObject.SlideInfoList[0].Segments = segments[..^1];
    }

    public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double time)
    {
        var localPosition = ToLocalSpace(screenSpacePosition);
        float angle = OriginPosition.AngleTo(localPosition);

        int targetLane = (int)Math.Round((angle - 22.5f) / 45);

        switch (PlacementActive)
        {
            case PlacementState.Waiting:
                if (Vector2.Distance(OriginPosition, localPosition) < 100)
                    break;

                HitObject.Lane = targetLane;
                break;

            case PlacementState.Active:
                // If the mapper maps the hold in reverse direction, we swap the start and end times to ensure correctness.
                HitObject.StartTime = Math.Min(commitStartTime, time);
                double endTime = Math.Max(commitStartTime, time);

                HitObject.SlideInfoList[0].Duration = endTime - HitObject.StartTime;

                // Yes this distance is higher, since slide bodies feel worse
                if (Vector2.Distance(OriginPosition, localPosition) < 200)
                    break;

                int startLane = HitObject.Lane + currentBody.RelativeEndLane - segments[^1].RelativeEndLane;

                var newSegment = segments[^1] with { RelativeEndLane = targetLane - startLane };

                if (!SlidePaths.CheckSlideValidity(newSegment))
                    break;

                segments[^1] = newSegment;
                updateSlideVisual();
                break;
        }

        return base.UpdateTimeAndPosition(screenSpacePosition, time);
    }
}
