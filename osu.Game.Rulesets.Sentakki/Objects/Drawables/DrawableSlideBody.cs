using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables;

public partial class DrawableSlideBody : DrawableSentakkiLanedHitObject
{
    public new DrawableSlide ParentHitObject => (DrawableSlide)base.ParentHitObject;
    public new SlideBody HitObject => (SlideBody)base.HitObject;

    private SlideBodyInfo slideBodyInfo => HitObject.SlideBodyInfo;

    // This slide body can only be interacted with iff the slidetap associated with this slide is judged
    public bool IsHittable
    {
        get
        {
            if (ParentHitObject.HitObject.TapType is Slide.TapTypeEnum.None)
                return Time.Current >= HitObject.StartTime;

            return ParentHitObject.SlideTaps.Child.Judged;
        }
    }

    public Container<DrawableSlideCheckpoint> SlideCheckpoints { get; private set; } = null!;

    public SlideVisual Slidepath { get; private set; } = null!;

    public Container<StarPiece> SlideStars { get; private set; } = null!;

    private float starProgress;

    public virtual float StarProgress
    {
        get => starProgress;
        set
        {
            starProgress = value;

            SlideStars[2].Position = slideBodyInfo.PositionAt(value);
            SlideStars[2].Rotation = slideBodyInfo.PositionAt(value - .01f).AngleTo(slideBodyInfo.PositionAt(value + .01f));

            if (slideBodyInfo.Segments[^1].Shape is not PathShapes.Fan) return;

            double fanStartProgress = slideBodyInfo.SegmentStartProgressFor(^1);

            if (!(starProgress >= fanStartProgress)) return;

            int startLane = slideBodyInfo.EndLane - 4;

            var startPos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, startLane);

            var endLeft = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, slideBodyInfo.EndLane - 1);
            var endRight = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, slideBodyInfo.EndLane - 1);

            SlideStars[0].Position = Interpolation.ValueAt(starProgress, startPos, endLeft, fanStartProgress, 1);
            SlideStars[1].Position = Interpolation.ValueAt(starProgress, startPos, endRight, fanStartProgress, 1);
        }
    }

    private static readonly Color4 inactive_color = Color4.LightGray.Darken(0.25f);
    private static readonly Color4 active_color = Color4.White;

    public DrawableSlideBody()
        : this(null)
    {
    }

    public DrawableSlideBody(SlideBody? hitObject)
        : base(hitObject)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Size = Vector2.Zero;
        Origin = Anchor.Centre;
        Anchor = Anchor.Centre;
        Rotation = -22.5f;

        AddRangeInternal([
            Slidepath = new SlideVisual { Colour = inactive_color },
            SlideStars = new ProxyableContainer<StarPiece>
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            },
            SlideCheckpoints = new Container<DrawableSlideCheckpoint>
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }
        ]);

        for (int i = 0; i < 3; ++i)
        {
            SlideStars.Add(new StarPiece
            {
                Alpha = 0,
                Scale = Vector2.Zero,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Position = MathExtensions.PointOnCircle(296.5f, 22.5f),
                RelativeSizeAxes = Axes.None,
            });
        }

        AccentColour.BindValueChanged(c => Colour = c.NewValue);

        OnNewResult += onNewResult;
        OnRevertResult += onRevertResult;
    }

    protected override void OnApply()
    {
        base.OnApply();
        Slidepath.Path = HitObject.SlideBodyInfo;
        Slidepath.Progress = 0;
        StarProgress = 0;
    }

    protected override void LoadSamples()
    {
        LoadBreakSamples();
    }

    protected override void OnFree()
    {
        base.OnFree();
        Slidepath.Free();
    }

    private void onNewResult(DrawableHitObject hitObject, JudgementResult result)
    {
        if (hitObject is not DrawableSlideCheckpoint checkpoint)
            return;

        if (!result.IsHit)
            return;

        Slidepath.Progress = checkpoint.HitObject.Progress;
    }

    private void onRevertResult(DrawableHitObject hitObject, JudgementResult result)
    {
        if (hitObject is not DrawableSlideCheckpoint checkpoint)
            return;

        Slidepath.Progress = checkpoint.ThisIndex == 0 ? 0 : SlideCheckpoints[checkpoint.ThisIndex - 1].HitObject.Progress;
    }

    protected override void Update()
    {
        base.Update();

        Slidepath.Colour = IsHittable ? active_color : inactive_color;
    }

    protected override void UpdateInitialTransforms()
    {
        base.UpdateInitialTransforms();
        Slidepath.PerformEntryAnimation(AnimationDuration.Value);
    }

    protected override void UpdateStartTimeStateTransforms()
    {
        base.UpdateStartTimeStateTransforms();

        // The primary star is always guaranteed to enter.
        SlideStars[2].FadeInFromZero(HitObject.ShootDelay).ScaleTo(1.25f, HitObject.ShootDelay);

        // This indirectly controls the animation of the stars following the path
        using (BeginDelayedSequence(HitObject.ShootDelay))
            this.TransformTo(nameof(StarProgress), 1f, (HitObject as IHasDuration).Duration - HitObject.ShootDelay);

        if (slideBodyInfo.Segments[^1].Shape is not PathShapes.Fan) return;

        int fanStartLane = slideBodyInfo.EndLane - 4;

        // Apply rotation to the extra stars
        SlideStars[0].RotateTo(fanStartLane + 45 * 3);
        SlideStars[1].RotateTo(fanStartLane + 45 * 5);

        // If the only segment is a fan, we fade the extra stars in the same way as the main star.
        if (slideBodyInfo.Segments.Count == 1)
        {
            SlideStars[0].FadeInFromZero(HitObject.ShootDelay).ScaleTo(1.25f, HitObject.ShootDelay);
            SlideStars[1].FadeInFromZero(HitObject.ShootDelay).ScaleTo(1.25f, HitObject.ShootDelay);
        }
        else // Otherwise, the suddenly pop-in.
        {
            using (BeginDelayedSequence(HitObject.ShootDelay + slideBodyInfo.MovementDuration * slideBodyInfo.SegmentStartProgressFor(^1)))
            {
                SlideStars[0].FadeIn().ScaleTo(1.25f);
                SlideStars[1].FadeIn().ScaleTo(1.25f);
            }
        }
    }

    protected override void CheckForResult(bool userTriggered, double timeOffset)
    {
        Debug.Assert(HitObject.HitWindows != null);

        // We start with the assumption that the player completed all checkpoints
        userTriggered = true;

        // If any of the checkpoints aren't complete, we consider the slide to be incomplete
        for (int i = 0; i < SlideCheckpoints.Count; ++i)
        {
            if (!SlideCheckpoints[i].Result.HasResult)
            {
                userTriggered = false;
                break;
            }
        }

        if (!userTriggered)
        {
            if (!HitObject.HitWindows.CanBeHit(timeOffset))
            {
                // Ensure that all of them have results
                foreach (var checkpoint in SlideCheckpoints)
                    checkpoint.ForcefullyMiss();

                // Apply a leniency if the player almost completed the slide
                if (SlideCheckpoints.Count(node => !node.Result.IsHit) <= 2 && SlideCheckpoints.Count > 2)
                    ApplyResult(hitResult: HitResult.Ok);
                else
                    ApplyResult(Result.Judgement.MinResult);
            }

            return;
        }

        var result = HitObject.HitWindows.ResultFor(timeOffset);

        // Give the player an OK for extremely early completion
        // This is also a safeguard for super late hits beyond the late windows, where the input may have occured prior to the late window being exceeded due to lag.
        if (result == HitResult.None)
            result = HitResult.Ok;

        // Give a perfect result if the star is intersecting with the last node
        // This is to preserve the expected invariant that following the star perfectly should guarantee a perfect judgement.
        if (timeOffset < 0)
        {
            double remainingTravelDistance = slideBodyInfo.SlideLength * (1 - StarProgress);

            if (remainingTravelDistance <= DrawableSlideCheckpointNode.DETECTION_RADIUS)
                result = HitResult.Perfect;
        }

        ApplyResult(result);
    }

    protected override void UpdateHitStateTransforms(ArmedState state)
    {
        base.UpdateHitStateTransforms(state);

        const double time_fade_miss = 400;
        const double time_fade_hit = 200;

        switch (state)
        {
            case ArmedState.Hit:
                Slidepath.PerformExitAnimation(time_fade_hit, HitStateUpdateTime);
                foreach (var star in SlideStars)
                    star.FadeOut(time_fade_hit);

                this.Delay(time_fade_hit).FadeOut();

                break;

            case ArmedState.Miss:
                Slidepath.PerformExitAnimation(time_fade_miss, Result.TimeAbsolute);

                foreach (var star in SlideStars)
                {
                    star.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                        .MoveToOffset(MathExtensions.PointOnCircle(100, star.Rotation), time_fade_miss, Easing.OutCubic);
                }

                this.FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint).FadeOut(time_fade_miss);
                break;
        }
    }

    protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
    {
        switch (hitObject)
        {
            case SlideCheckpoint checkpoint:
                return new DrawableSlideCheckpoint(checkpoint)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoBindable = { BindTarget = AutoBindable },
                };
        }

        return base.CreateNestedHitObject(hitObject);
    }

    protected override void AddNestedHitObject(DrawableHitObject hitObject)
    {
        base.AddNestedHitObject(hitObject);

        switch (hitObject)
        {
            case DrawableSlideCheckpoint node:
                SlideCheckpoints.Add(node);
                break;
        }
    }

    protected override void ClearNestedHitObjects()
    {
        base.ClearNestedHitObjects();
        SlideCheckpoints.Clear(false);
    }

    private partial class ProxyableContainer<T> : Container<T> where T : Drawable
    {
        public override bool RemoveWhenNotAlive => false;
    }
}
