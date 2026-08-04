using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy;

public partial class LegacyPlayfieldRing : BeatSyncedContainer
{
    private Drawable ringDrawable = null!;
    private Drawable pulseRing = null!;

    public override bool RemoveCompletedTransforms => false;

    private readonly Bindable<ColorOption> ringColor = new Bindable<ColorOption>();
    private IBindable<StarDifficulty> beatmapDifficulty = null!;

    public LegacyPlayfieldRing()
    {
        RelativeSizeAxes = Axes.Both;
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
    }

    [Resolved]
    private ISkinSource skin { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load(SentakkiRulesetConfigManager? settings, IBeatmap? beatmap, BeatmapDifficultyCache? difficultyCache)
    {
        // If we get here, we know the assets exist
        pulseRing = skin.GetAnimation("sentakki/playfield-ring", true, true, true, "/")!;
        pulseRing.RelativeSizeAxes = Axes.Both;
        pulseRing.Size = new Vector2(1.0133333f);
        pulseRing.Anchor = Anchor.Centre;
        pulseRing.Origin = Anchor.Centre;
        pulseRing.Alpha = 0;

        ringDrawable = skin.GetAnimation("sentakki/playfield-ring", true, true, true, "/")!;
        ringDrawable.RelativeSizeAxes = Axes.Both;
        ringDrawable.Size = Vector2.One;
        ringDrawable.Anchor = Anchor.Centre;
        ringDrawable.Origin = Anchor.Centre;

        InternalChildren = [
            pulseRing,
            ringDrawable,
        ];

        settings?.BindWith(SentakkiRulesetSettings.RingColor, ringColor);

        if (beatmap is null || difficultyCache is null)
            return;

        // handle colouring of playfield elements
        beatmapDifficulty = difficultyCache.GetBindableDifficulty(beatmap.BeatmapInfo);
        beatmapDifficulty.BindValueChanged(_ => updateColours());
    }

    protected override void LoadComplete()
    {

        ringColor.BindValueChanged(_ => updateColours(), true);
    }

    private bool wasKiai;

    protected override void Update()
    {
        base.Update();

        if (EffectPoint.KiaiMode && !wasKiai)
        {
            bool isNearEffectPoint = Math.Abs(BeatSyncSource.Clock.CurrentTime - EffectPoint.Time) < 500;

            if (isNearEffectPoint)
                Pulse(BeatSyncSource.CurrentAmplitudes.Average);
        }

        wasKiai = EffectPoint.KiaiMode;
    }

    protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
    {
        base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

        if (!effectPoint.KiaiMode)
            return;

        if (((beatIndex * 4) % timingPoint.TimeSignature.Numerator) == 0)
            Pulse(amplitudes.Average);
    }


    [Resolved]
    private OsuColour colours { get; set; } = null!;

    private void updateColours()
    {
        switch (ringColor.Value)
        {
            case ColorOption.Difficulty:
                double starRating = beatmapDifficulty.Value.Stars;
                var colour = colours.ForStarDifficulty(starRating);

                // Normalize the colors to make sure the ring is actually visible
                colour = Interpolation.ValueAt(0.5f, colour, new HSPAColour(colour) { P = 0.6f }.ToColor4(), 0, 1);

                this.FadeColour(colour, 200);
                break;

            case ColorOption.Skin:
                this.FadeColour(skin.GetConfig<GlobalSkinColours, Color4>(GlobalSkinColours.MenuGlow)?.Value ?? Color4.White, 200);
                break;

            default:
                this.FadeColour(Color4.White, 200);
                break;
        }
    }

    public void Pulse(float amplitude = 1)
    {
        pulseRing.FinishTransforms();
        pulseRing.ClearTransforms();
        pulseRing.ScaleTo(1).FadeTo(0.5f * amplitude).ScaleTo(1.06f, 200).FadeOut(200);
    }
}
