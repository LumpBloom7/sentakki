using System;
using System.Numerics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Sentakki.HUD;
using osu.Game.Screens.Play.HUD;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Argon.HUD;

public partial class ArgonContextualCounter : ContextualCounter
{
    [SettingSource("Show label")]
    public Bindable<bool> ShowLabel { get; } = new();

    [SettingSource("Wireframe Opacity")]
    public BindableFloat WireframeOpacity { get; } = new(0.25f)
    {
        Precision = 0.01f,
        MinValue = 0,
        MaxValue = 1,
    };

    protected override Drawable CreateComponentFor(DefaultDisplay mode) => mode switch
    {
        DefaultDisplay.Accuracy => new ArgonAccuracyCounter
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,

            ShowLabel = { BindTarget = ShowLabel },
            WireframeOpacity = { BindTarget = WireframeOpacity },
            AccuracyDisplay = { BindTarget = AccuracyDisplayMode }
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

    protected override Drawable? CreateOverrideCounterFor(IProvideContextualCounter mod)
    {
        switch (mod.CounterBindable)
        {
            case BindableInt bindableInt:
                return new RollingArgonCounterTextComponent<int>(Anchor.TopLeft, mod.CounterLabel)
                {
                    WireframeOpacity = { BindTarget = WireframeOpacity },
                    ShowLabel = { BindTarget = ShowLabel },
                    WireframeTemplate = new string('#', getDigitsRequiredForDisplay(bindableInt.MaxValue)),
                    Current = { BindTarget = bindableInt }
                };
        }

        return null;
    }

    private int getDigitsRequiredForDisplay(int c)
    {
        int digitsRequired = 0;

        while ((c /= 10) > 0)
            digitsRequired++;
        return digitsRequired;
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
