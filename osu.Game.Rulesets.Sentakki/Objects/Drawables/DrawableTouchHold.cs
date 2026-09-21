using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables;

public partial class DrawableTouchHold : DrawableSentakkiHitObject
{
    public new TouchHold HitObject => (TouchHold)base.HitObject;

    public override bool HandlePositionalInput => true;

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => TouchHoldBody.ReceivePositionalInputAt(screenSpacePos);

    public TouchHoldBody TouchHoldBody = null!;

    private PausableSkinnableSound holdSample = null!;

    [Cached]
    private Bindable<IReadOnlyList<Color4>> colourPalette = new Bindable<IReadOnlyList<Color4>>();

    private readonly IBindable<Vector2> positionBindable = new Bindable<Vector2>();

    private Container<DrawableTouchHoldHead> headContainer = null!;
    private DrawableTouchHoldHead head => headContainer.Child;

    public DrawableTouchHold()
        : this(null)
    {
    }

    public DrawableTouchHold(TouchHold? hitObject)
        : base(hitObject)
    {
    }

    protected override void OnApply()
    {
        base.OnApply();
        colourPalette.BindTo(HitObject.ColourPaletteBindable);
        positionBindable.BindTo(HitObject.PositionBindable);
        timeNotHeld = 0;
        isHitting.Value = false;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        if (DrawableSentakkiRuleset is not null)
            AnimationDuration.BindTo(DrawableSentakkiRuleset?.AdjustedTouchAnimDuration);

        Colour = Color4.SlateGray;
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        AddRangeInternal(
        [
            headContainer = new Container<DrawableTouchHoldHead> { RelativeSizeAxes = Axes.Both },
            TouchHoldBody = new TouchHoldBody(),
            holdSample = new PausableSkinnableSound
            {
                Volume = { Value = 1 },
                Looping = true,
                Frequency = { Value = 1 }
            },
        ]);

        positionBindable.BindValueChanged(v => Position = v.NewValue);
        pressedCount.BindValueChanged(onPressedCountChanged);
    }

    protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
    {
        switch (hitObject)
        {
            case TouchHold.TouchHoldHead head:
                return new DrawableTouchHoldHead(head)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    AutoBindable = { BindTarget = AutoBindable }
                };
        }

        return base.CreateNestedHitObject(hitObject);
    }

    protected override void AddNestedHitObject(DrawableHitObject hitObject)
    {
        base.AddNestedHitObject(hitObject);

        switch (hitObject)
        {
            case DrawableTouchHoldHead head:
                headContainer.Child = head;
                break;
        }
    }

    protected override void ClearNestedHitObjects()
    {
        base.ClearNestedHitObjects();
        headContainer.Clear(false);
    }

    protected override void LoadSamples()
    {
        base.LoadSamples();

        holdSample.Samples = [.. HitObject.CreateHoldSample().Cast<ISampleInfo>()];
        holdSample.Frequency.Value = 1;
    }

    public override void StopAllSamples()
    {
        base.StopAllSamples();
        holdSample.Stop();
    }

    [Resolved]
    private OsuColour colours { get; set; } = null!;

    protected override void OnFree()
    {
        base.OnFree();

        holdSample.ClearSamples();
        colourPalette.UnbindFrom(HitObject.ColourPaletteBindable);
        positionBindable.UnbindFrom(HitObject.PositionBindable);
    }

    protected override void UpdateInitialTransforms()
    {
        base.UpdateInitialTransforms();
        double animTime = AnimationDuration.Value * 0.8;
        double fadeTime = AnimationDuration.Value * 0.2;

        TouchHoldBody.FadeInFromZero(fadeTime).ScaleTo(1);

        using (BeginDelayedSequence(fadeTime))
            TouchHoldBody.ResizeTo(80, animTime, Easing.InCirc);
    }

    protected override void UpdateStartTimeStateTransforms()
    {
        base.UpdateStartTimeStateTransforms();

        TouchHoldBody.CentrePiece.FadeOut();
        TouchHoldBody.CompletedCentre.FadeIn();
        TouchHoldBody.ProgressPiece.TransformBindableTo(TouchHoldBody.ProgressPiece.ProgressBindable, 1, ((IHasDuration)HitObject).Duration);
    }

    [Cached]
    private readonly Bindable<bool> isHitting = new Bindable<bool>();

    private double timeNotHeld;

    private Bindable<int> pressedCount = new Bindable<int>();

    protected override void Update()
    {
        base.Update();

        pressedCount.Value = countActiveTouchPoints();

        if (AllJudged)
        {
            // Remove alterations to NoteBody colour
            Colour = Color4.White;
            return;
        }

        // Ensure that the note colour is correct prior to the start time
        if (Time.Current < HitObject.StartTime)
        {
            Colour = Color4.White;
            return;
        }

        if (Auto)
        {
            // If auto is within the hittable time, attempt to hit it
            // HACK: In editor context, frame stability is not enforced, this could potentially lead to 0 duration slides being missed as we never ever visit the window.
            // We resolve this by giving autoplay a bit more leniency. In practice nothing should change for regular autoplay.
            double missWindow = HitObject.HitWindows.WindowFor(HitResult.Miss);

            if (Time.Current >= HitObject.StartTime && Time.Current < HitObject.EndTime + missWindow)
            {
                if (!isHitting.Value)
                    head.UpdateResult();

                isHitting.Value = true;
            }
        }

        if (!isHitting.Value)
        {
            holdSample.Stop();

            // Grey the note to indicate that it isn't being held
            Colour = Interpolation.ValueAt(
                Math.Clamp(Time.Current, HitObject.StartTime, HitObject.StartTime + 100),
                Color4.White, Color4.SlateGray,
                HitObject.StartTime, HitObject.StartTime + 100, Easing.OutSine);


            timeNotHeld += Time.Elapsed;

            if (head.AllJudged && timeNotHeld >= 400)
            {
                if (!Judged)
                    ApplyMinResult();

                return;
            }

            return;
        }

        timeNotHeld = 0;

        if (!holdSample.RequestedPlaying)
            holdSample.Play();

        holdSample.Frequency.Value = 0.5 + ((Time.Current - HitObject.StartTime) / ((IHasDuration)HitObject).Duration) * 0.5f;
        Colour = Color4.White;
    }

    protected override void CheckForResult(bool userTriggered, double timeOffset)
    {
        if (userTriggered)
            return;

        double perfectWindow = HitObject.HitWindows.WindowFor(HitResult.Perfect);
        if (timeOffset > 0 && isHitting.Value)
        {
            ApplyResult(HitResult.Perfect);
        }
        else if (head.AllJudged && timeOffset >= -perfectWindow && !isHitting.Value)
        {
            // If the user is not holding the note, use the unheld duration to determine an appropriate result
            var earlyReleaseResult = HitObject.HitWindows.ResultFor(timeNotHeld + Math.Abs(timeOffset));

            if (earlyReleaseResult <= HitResult.None)
                earlyReleaseResult = HitResult.Miss;

            ApplyResult(earlyReleaseResult);
        }

        return;
    }
    protected override void UpdateHitStateTransforms(ArmedState state)
    {
        base.UpdateHitStateTransforms(state);
        const double time_fade_miss = 400;

        switch (state)
        {
            case ArmedState.Hit:
                TouchHoldBody.FadeOut();
                this.FadeOut().OnComplete(_ => holdSample.Stop());
                break;

            case ArmedState.Miss:
                TouchHoldBody.ScaleTo(.0f, time_fade_miss).FadeOut(time_fade_miss);
                this.Delay(time_fade_miss).FadeOut().OnComplete(_ => holdSample.Stop());
                break;
        }

        Expire();
    }

    [Resolved]
    private SentakkiInputManager sentakkiInputManager { get; set; } = null!;

    private int countActiveTouchPoints()
    {
        var touchInput = sentakkiInputManager.CurrentState.Touch;
        int count = 0;

        if (ReceivePositionalInputAt(sentakkiInputManager.CurrentState.Mouse.Position))
        {
            foreach (var item in sentakkiInputManager.PressedActions)
            {
                if (item < SentakkiAction.Key1)
                    ++count;
            }
        }

        foreach (TouchSource source in touchInput.ActiveSources)
        {
            if (touchInput.GetTouchPosition(source) is Vector2 touchPosition && ReceivePositionalInputAt(touchPosition))
                ++count;
        }

        return count;
    }

    private void onPressedCountChanged(ValueChangedEvent<int> pressedCount)
    {
        if (pressedCount.NewValue > pressedCount.OldValue)
            onTouchPressed();
        else
            onTouchReleased();
    }

    private void onTouchPressed()
    {
        if (AllJudged)
            return;

        double timeOffset = Time.Current - HitObject.StartTime;

        if (timeOffset < -head.HitObject.HitWindows.WindowFor(HitResult.Perfect))
            return;

        head.UpdateResult();
        isHitting.Value = true;

    }

    private void onTouchReleased()
    {
        if (AllJudged)
            return;

        if (!isHitting.Value)
            return;

        if (pressedCount.Value > 0)
            return;

        UpdateResult(true);
        isHitting.Value = false;

        if (!AllJudged)
            Colour = Color4.Gray;
    }
}
