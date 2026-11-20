using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.HUD;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy.HUD;

public partial class LegacyContextualCounter : ContextualCounter
{
    protected override Drawable CreateComponentFor(DefaultDisplay mode) => mode switch
    {
        DefaultDisplay.Combo => new SimplerLegacyDefaultComboCounter
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        },
        DefaultDisplay.Accuracy => new LegacyAccuracyCounter
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            AccuracyDisplay = { BindTarget = AccuracyDisplayMode }
        },
        DefaultDisplay.Score => new LegacyScoreCounter
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
            case Bindable<int> bindableInt:
                return new NonProportionalLegacyRollingCounter()
                {
                    Current = { BindTarget = bindableInt }
                };
            default:
                return null;
        }
    }

    private partial class NonProportionalLegacyRollingCounter : LegacyRollingCounter
    {
        protected override double RollingDuration => 250;
        protected override bool IsRollingProportional => false;

        public NonProportionalLegacyRollingCounter()
            : base(LegacyFont.Combo)
        {
        }
    }

    private partial class SimplerLegacyDefaultComboCounter : LegacyRollingCounter
    {
        protected override double RollingDuration => 0;
        protected override bool IsRollingProportional => false;

        public SimplerLegacyDefaultComboCounter()
            : base(LegacyFont.Combo)
        {
        }

        [BackgroundDependencyLoader]
        private void load(ScoreProcessor scoreProcessor)
        {
            Current.BindTo(scoreProcessor.Combo);
        }
    }
}
