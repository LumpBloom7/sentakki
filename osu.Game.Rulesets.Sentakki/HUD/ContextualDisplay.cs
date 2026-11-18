using System;
using System.Linq;
using System.Numerics;
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

        applyOverrideCounterFromMods();
        DefaultDisplayMode.BindValueChanged(displayModeChanged, true);
    }

    private void recreateCounter()
    {
        InternalChild = CreateComponentFor(DefaultDisplayMode.Value);
    }

    protected virtual Drawable CreateComponentFor(DefaultDisplay mode) => mode switch
    {
        DefaultDisplay.Accuracy => new ArgonAccuracyCounter
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,

            ShowLabel = { BindTarget = ShowLabel },
            WireframeOpacity = { BindTarget = WireframeOpacity },
        },

        DefaultDisplay.Combo => new ArgonComboCounter
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,

            ShowLabel = { BindTarget = ShowLabel },
            WireframeOpacity = { BindTarget = WireframeOpacity },
        },

        DefaultDisplay.Score => new ArgonScoreCounter
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,

            ShowLabel = { BindTarget = ShowLabel },
            WireframeOpacity = { BindTarget = WireframeOpacity },
        },
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };

    private void displayModeChanged(ValueChangedEvent<DefaultDisplay> defaultDisplay)
    {
        if (overrideCounterBindable is not null)
            return;

        recreateCounter();
    }

    [Resolved]
    private Player player { get; set; } = null!;

    private IBindable? overrideCounterBindable { get; set; }

    private void applyOverrideCounterFromMods()
    {
        var mod = player.GameplayState.Mods.OfType<IProvideContextualCounter>().SingleOrDefault();

        if (mod is null)
            return;

        var counterBindable = mod.CounterBindable;

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

                InternalChild = rollingCounter;
                break;

            default:
                return;
        }

        overrideCounterBindable = counterBindable;
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
