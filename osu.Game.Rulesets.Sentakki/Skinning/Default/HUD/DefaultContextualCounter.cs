using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Sentakki.HUD;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default.HUD;

public partial class DefaultContextualCounter : ContextualCounter
{
    protected override Drawable CreateComponentFor(DefaultDisplay mode) => mode switch
    {
        DefaultDisplay.Accuracy => new DefaultAccuracyCounter()
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            AccuracyDisplay = { BindTarget = AccuracyDisplayMode }
        },

        DefaultDisplay.Combo => new DefaultComboCounter
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        },

        DefaultDisplay.Score => new DefaultScoreCounter
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        },

        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };

    protected override Drawable? CreateOverrideCounterFor(IProvideContextualCounter mod)
    {
        switch (mod.CounterBindable)
        {
            case BindableInt bindableInt:
                return new DefaultRollingCounter<int>
                {
                    Current = { BindTarget = bindableInt }
                };
            default:
                return null;
        }
    }

    private partial class DefaultRollingCounter<T> : RollingCounter<T> where T : struct, IEquatable<T>
    {
        protected override double RollingDuration => 375;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.BlueLighter;
        }

        protected override OsuSpriteText CreateSpriteText()
            => base.CreateSpriteText().With(s => s.Font = s.Font.With(size: 20f));

        protected override LocalisableString FormatCount(T count) => $@"{count}";
    }
}
