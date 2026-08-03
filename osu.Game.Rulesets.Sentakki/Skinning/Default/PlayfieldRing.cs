using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default;

public partial class PlayfieldRing : BeatSyncedContainer
{

    private readonly Container spawnIndicator;
    private readonly Drawable pulseRing;

    public override bool RemoveCompletedTransforms => false;

    public PlayfieldRing()
    {
        RelativeSizeAxes = Axes.Both;
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        InternalChildren =
        [
            pulseRing = new CircularProgress
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(608),
                Colour = Color4.White,
                InnerRadius = 12f / 304f,
                Progress = 1,
                Alpha = 0,
            },
            new CircularProgress
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(600),
                InnerRadius = 8f / 300f,
                Progress = 1,
                Colour = Color4.Gray
            },
            new CircularProgress
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(596),
                InnerRadius = 4f / 298f,
                Progress = 1,
                Colour = Color4.White
            },
            spawnIndicator = new Container
            {
                Name = "Spawn indicators",
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0
            }
        ];

        // Add dots to the actual ring
        foreach (float pathAngle in SentakkiPlayfield.LANEANGLES)
        {
            AddInternal(new DotPiece
            {
                Position = MathExtensions.PointOnCircle(SentakkiPlayfield.INTERSECTDISTANCE, pathAngle),
            });

            spawnIndicator.Add(new DotPiece(size: new Vector2(16, 8))
            {
                Rotation = pathAngle,
                Position = MathExtensions.PointOnCircle(SentakkiPlayfield.NOTESTARTDISTANCE, pathAngle),
            });
        }
    }

    public readonly Bindable<float> RingOpacity = new Bindable<float>(1);
    public readonly Bindable<bool> NoteStartIndicators = new Bindable<bool>();

    private readonly Bindable<ColorOption> ringColor = new Bindable<ColorOption>();
    private IBindable<StarDifficulty> beatmapDifficulty = null!;

    [BackgroundDependencyLoader]
    private void load(SentakkiRulesetConfigManager? settings, IBeatmap? beatmap, BeatmapDifficultyCache? difficultyCache)
    {
        settings?.BindWith(SentakkiRulesetSettings.RingColor, ringColor);

        settings?.BindWith(SentakkiRulesetSettings.RingOpacity, RingOpacity);
        RingOpacity.BindValueChanged(opacity => Alpha = opacity.NewValue, true);

        settings?.BindWith(SentakkiRulesetSettings.ShowNoteStartIndicators, NoteStartIndicators);
        NoteStartIndicators.BindValueChanged(opacity => spawnIndicator.FadeTo(Convert.ToSingle(opacity.NewValue), 200), true);

        if (beatmap is null || difficultyCache is null)
            return;

        // handle colouring of playfield elements
        beatmapDifficulty = difficultyCache.GetBindableDifficulty(beatmap.BeatmapInfo);
    }

    protected override void LoadComplete()
    {
        // These usually animate in, but they shouldn't if the game was started with it already on
        spawnIndicator.FinishTransforms(true);
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

    [Resolved]
    private ISkinSource skin { get; set; } = null!;

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
