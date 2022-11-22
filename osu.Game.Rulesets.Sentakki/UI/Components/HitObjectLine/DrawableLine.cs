using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets.Sentakki.Configuration;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI.Components.HitObjectLine
{
    public class DrawableLine : PoolableDrawable
    {
        public override bool RemoveCompletedTransforms => false;

        public LineLifetimeEntry Entry = null!;

        private CircularProgress line = null!;

        public DrawableLine()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Origin = Anchor.Centre;
            Scale = new Vector2(.22f);
            Alpha = 0;
        }

        private readonly BindableDouble animationDuration = new BindableDouble(1000);

        [BackgroundDependencyLoader]
        private void load(SentakkiRulesetConfigManager? sentakkiConfigs)
        {
            sentakkiConfigs?.BindWith(SentakkiRulesetSettings.AnimationDuration, animationDuration);
            animationDuration.BindValueChanged(_ => resetAnimation());

            AddInternal(line = new CircularProgress()
            {
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                InnerRadius = 0.026f,
                RoundedCaps = true,
                Alpha = 0.8f,
            });
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Colour = Entry.Colour;
            Rotation = Entry.Rotation;
            line.Current.Value = Entry.AngleRange;
            resetAnimation();
        }

        private void resetAnimation()
        {
            if (!IsInUse) return;
            ApplyTransformsAt(double.MinValue);
            ClearTransforms();
            using (BeginAbsoluteSequence(Entry.StartTime - Entry.AdjustedAnimationDuration))
                this.FadeIn(Entry.AdjustedAnimationDuration / 2).Then().ScaleTo(1, Entry.AdjustedAnimationDuration / 2).Then().FadeOut();
        }
    }
}
