using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Sentakki.Configuration;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI.Components.HitObjectLine
{
    public class DrawableLine : PoolableDrawable
    {
        public override bool RemoveCompletedTransforms => false;

        public LineLifetimeEntry Entry;

        public LineType Type;
        public DrawableLine()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Origin = Anchor.Centre;
            Scale = new Vector2(.22f);
            Alpha = 0;
        }

        private readonly BindableDouble animationDuration = new BindableDouble(1000);

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfigs, TextureStore textures)
        {
            sentakkiConfigs?.BindWith(SentakkiRulesetSettings.AnimationDuration, animationDuration);
            animationDuration.BindValueChanged(_ => resetAnimation());

            AddInternal(new Sprite()
            {
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Texture = textures.Get(LineTexturePath)
            });
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Colour = Entry.Colour;
            Rotation = Entry.Rotation;
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

        public string LineTexturePath
        {
            get
            {
                switch (Type)
                {
                    case LineType.Single:
                        return "Lines/90";
                    case LineType.OneAway:
                        return "Lines/135";
                    case LineType.TwoAway:
                        return "Lines/180";
                    case LineType.ThreeAway:
                        return "Lines/225";
                    case LineType.FullCircle:
                        return "Lines/360";
                    default:
                        return "";
                }
            }
        }
    }
}
