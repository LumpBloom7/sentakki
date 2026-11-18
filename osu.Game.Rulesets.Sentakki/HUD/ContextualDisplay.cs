using System;
using System.Linq;
using System.Numerics;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.HUD;

public partial class ArgonContextualDisplay : CompositeDrawable, ISerialisableDrawable
{
    public bool UsesFixedAnchor { get; set; }

    private Container defaultCounters = null!;
    private Container overrideCounters = null!;

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
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        AutoSizeAxes = Axes.Both;
        InternalChildren =
        [
            defaultCounters = new Container
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                Children =
                [
                    accuracyCounter = new ArgonAccuracyCounter
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 0,

                        ShowLabel = { BindTarget = ShowLabel },
                        WireframeOpacity = { BindTarget = WireframeOpacity },
                        AccuracyDisplay = { BindTarget = AccuracyDisplayMode },
                    },
                    comboCounter = new ArgonComboCounter
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 0,

                        ShowLabel = { BindTarget = ShowLabel },
                        WireframeOpacity = { BindTarget = WireframeOpacity },
                    },
                    scoreCounter = new ArgonScoreCounter
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 0,

                        ShowLabel = { BindTarget = ShowLabel },
                        WireframeOpacity = { BindTarget = WireframeOpacity },
                    }
                ]
            },

            overrideCounters = new Container
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0
            }
        ];

        DefaultDisplayMode.BindValueChanged(displayModeChanged, true);
        applyOverrideCounterFromMods();
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

    [Resolved]
    private Player player { get; set; } = null!;

    [UsedImplicitly]
    private IBindable? overrideCounterBindable { get; set; }

    private void applyOverrideCounterFromMods()
    {
        var mod = player.GameplayState.Mods.OfType<IProvideContextualCounter>().SingleOrDefault();

        if (mod is null)
            return;

        var counterBindable = mod.CounterBindable;

        Drawable counter;

        // Add more conditions as needed
        switch (counterBindable)
        {
            case BindableInt bindableInt:
                var rollingCounter = new RollingArgonCounterTextComponent<int>(Anchor.TopLeft, mod.CounterLabel)
                {
                    WireframeOpacity = { BindTarget = WireframeOpacity },
                    ShowLabel = { BindTarget = ShowLabel },
                    WireframeTemplate = new string('#', getDigitsRequiredForDisplay(bindableInt.MaxValue)),
                    Current = { BindTarget = bindableInt }
                };

                counter = rollingCounter;
                break;

            default:
                return;
        }

        overrideCounterBindable = counterBindable;

        overrideCounters.Add(counter);
        defaultCounters.Hide();
        overrideCounters.Show();
    }

    private int getDigitsRequiredForDisplay(int c)
    {
        int digitsRequired = 0;

        while ((c /= 10) > 0)
            digitsRequired++;
        return digitsRequired;
    }

    public enum DefaultDisplay
    {
        Combo,
        Accuracy,
        Score,
    }

    private partial class RollingArgonCounterTextComponent<T> : RollingCounter<T> where T : struct, IEquatable<T>, IComparisonOperators<T, T, bool>
    {
        private readonly ArgonCounterTextComponent counterText;

        public IBindable<float> WireframeOpacity => counterText.WireframeOpacity;
        public Bindable<bool> ShowLabel => counterText.ShowLabel;

        protected override double RollingDuration => 250;

        public string WireframeTemplate
        {
            get => counterText.WireframeTemplate;
            set => counterText.WireframeTemplate = value;
        }

        public RollingArgonCounterTextComponent(Anchor anchor, LocalisableString? label = null)
        {
            counterText = new ArgonCounterTextComponent(anchor, label);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Current.BindValueChanged(v =>
            {
                bool wasMiss = v.NewValue < v.OldValue;
                if (!wasMiss)
                    return;

                counterText.FinishTransforms(true);

                float newScale = Math.Clamp(counterText.NumberContainer.Scale.X * 0.8f, 0.6f, 1.4f);

                float duration = wasMiss ? 2000 : 500;

                counterText.NumberContainer
                           .ScaleTo(new osuTK.Vector2(newScale))
                           .ScaleTo(osuTK.Vector2.One, duration, Easing.OutQuint);

                if (wasMiss)
                    counterText.FlashColour(Color4.Red, duration, Easing.OutQuint);
            });
        }

        protected override IHasText CreateText() => counterText;
    }
}
