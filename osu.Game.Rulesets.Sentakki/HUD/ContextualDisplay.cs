using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.HUD;

[Cached]
public partial class ArgonContextualDisplay : CompositeDrawable, ISerialisableDrawable
{
    public bool UsesFixedAnchor { get; set; } = false;

    private ArgonAccuracyCounter accuracyCounter = null!;
    private ArgonComboCounter comboCounter = null!;
    private ArgonScoreCounter scoreCounter = null!;

    [SettingSource("Show label")]
    public Bindable<bool> ShowLabel { get; } = new();

    [SettingSource("Wireframe Opacity")]
    public BindableFloat WireframeOpacity { get; } = new(0.25f)
    {
        Precision = 0.01f,
        MinValue = 0,
        MaxValue = 1,
    };

    [SettingSource("Accuracy display mode")]
    public Bindable<GameplayAccuracyCounter.AccuracyDisplayMode> AccuracyDisplayMode { get; } = new();

    [SettingSource("Default display")]
    public Bindable<DefaultDisplay> DefaultDisplayMode { get; } = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        AutoSizeAxes = Axes.Both;
        InternalChildren =
        [
            accuracyCounter = new ArgonAccuracyCounter()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0,

                ShowLabel = { BindTarget = ShowLabel },
                WireframeOpacity = { BindTarget = WireframeOpacity },
                AccuracyDisplay = { BindTarget = AccuracyDisplayMode },
            },
            comboCounter = new ArgonComboCounter()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0,

                ShowLabel = { BindTarget = ShowLabel },
                WireframeOpacity = { BindTarget = WireframeOpacity },
            },
            scoreCounter = new ArgonScoreCounter()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0,

                ShowLabel = { BindTarget = ShowLabel },
                WireframeOpacity = { BindTarget = WireframeOpacity },
            }
        ];

        DefaultDisplayMode.BindValueChanged(displayModeChanged, true);
    }

    private void displayModeChanged(ValueChangedEvent<DefaultDisplay> defaultDisplay)
    {
        drawableFor(defaultDisplay.OldValue).Hide();
        drawableFor(defaultDisplay.NewValue).Show();

        return;

        Drawable drawableFor(DefaultDisplay mode) => mode switch
        {
            DefaultDisplay.Accuracy => accuracyCounter,
            DefaultDisplay.Combo => comboCounter,
            DefaultDisplay.Score => scoreCounter,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    public enum DefaultDisplay
    {
        Combo,
        Accuracy,
        Score,
    }
}
