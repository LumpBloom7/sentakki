using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
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
            TouchHoldBody = new TouchHoldBody(),
            holdSample = new PausableSkinnableSound
            {
                Volume = { Value = 1 },
                Looping = true,
                Frequency = { Value = 1 }
            }
        ]);

        positionBindable.BindValueChanged(v => Position = v.NewValue);
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
        isHitting.Value = false;
        totalHoldTime = 0;
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

    private double totalHoldTime;

    private bool isHittable => Time.Current >= HitObject.StartTime - 150 && Time.Current <= HitObject.GetEndTime();
    private bool withinActiveTime => Time.Current >= HitObject.StartTime && Time.Current <= HitObject.GetEndTime();

    private int pressedCount;

    protected override void Update()
    {
        base.Update();

        int updatedPressedCounts = countActiveTouchPoints();

        if (isHittable && (updatedPressedCounts > pressedCount || Auto))
            isHitting.Value = true;
        else if (!isHittable || updatedPressedCounts == 0)
            isHitting.Value = false;

        pressedCount = updatedPressedCounts;

        if (!isHitting.Value)
        {
            holdSample.Stop();

            // Grey the note to indicate that it isn't being held
            Colour = Interpolation.ValueAt(
                Math.Clamp(Time.Current, HitObject.StartTime, HitObject.StartTime + 100),
                Color4.White, Color4.SlateGray,
                HitObject.StartTime, HitObject.StartTime + 100, Easing.OutSine);
            return;
        }

        if (!withinActiveTime)
            return;

        if (!holdSample.RequestedPlaying)
            holdSample.Play();

        totalHoldTime = Math.Clamp(totalHoldTime + Time.Elapsed, 0, ((IHasDuration)HitObject).Duration);
        holdSample.Frequency.Value = 0.5 + totalHoldTime / ((IHasDuration)HitObject).Duration;
        Colour = Color4.White;
    }

    protected override void CheckForResult(bool userTriggered, double timeOffset)
    {
        if (Time.Current < ((IHasDuration)HitObject).EndTime) return;

        double result = totalHoldTime / ((IHasDuration)HitObject).Duration;

        HitResult resultType;

        if (result >= 0.90)
            resultType = HitResult.Perfect;
        else if (result >= 0.75)
            resultType = HitResult.Great;
        else if (result >= 0.5)
            resultType = HitResult.Good;
        else if (result >= 0.25)
            resultType = HitResult.Ok;
        else
            resultType = HitResult.Miss;

        // This is specifically to accommodate the threshold setting in HR
        if (!HitObject.HitWindows.IsHitResultAllowed(resultType))
            resultType = HitResult.Miss;

        AccentColour.Value = colours.ForHitResult(resultType);
        ApplyResult(resultType);
    }

    protected override void UpdateHitStateTransforms(ArmedState state)
    {
        base.UpdateHitStateTransforms(state);
        const double time_fade_miss = 400;

        switch (state)
        {
            case ArmedState.Hit:
                TouchHoldBody.FadeOut();
                this.FadeOut();
                break;

            case ArmedState.Miss:
                TouchHoldBody.ScaleTo(.0f, time_fade_miss).FadeOut(time_fade_miss);
                this.Delay(time_fade_miss).FadeOut();
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
}
