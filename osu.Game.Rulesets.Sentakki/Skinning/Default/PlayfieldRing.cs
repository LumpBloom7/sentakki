using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default;

public partial class PlayfieldRing : CompositeDrawable
{
    private readonly Container spawnIndicator;

    public PlayfieldRing()
    {
        RelativeSizeAxes = Axes.Both;
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        InternalChildren =
        [
            new PlayfieldVisualisation(),
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
    private readonly Bindable<bool> kiaiEffect = new Bindable<bool>(true);

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

        settings?.BindWith(SentakkiRulesetSettings.KiaiEffects, kiaiEffect);

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

    public void KiaiBeat()
    {
        if (!kiaiEffect.Value) return;

        FinishTransforms();
        this.ScaleTo(1.01f, 100).Then().ScaleTo(1, 100);
    }
}
