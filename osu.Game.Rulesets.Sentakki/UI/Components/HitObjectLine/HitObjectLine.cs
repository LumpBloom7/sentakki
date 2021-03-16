using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Configuration;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI.Components.HitObjectLine
{
    public class HitObjectLine : PoolableDrawable
    {
        public override double LifetimeStart
        {
            get => base.LifetimeStart;
            set => setLifetime(value, LifetimeEnd);
        }
        public override double LifetimeEnd
        {
            get => base.LifetimeEnd;
            set => setLifetime(LifetimeStart, value);
        }


        public override bool RemoveCompletedTransforms => false;

        public LineLifetimeEntry Entry;

        private Sprite sprite;
        public HitObjectLine()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = new Vector2(.22f);
            Size = new Vector2(600);
            Alpha = 0;
        }

        private readonly BindableDouble animationDuration = new BindableDouble(1000);

        private TextureStore textures;

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfigs, TextureStore textureStore)
        {
            textures = textureStore;
            sentakkiConfigs?.BindWith(SentakkiRulesetSettings.AnimationDuration, animationDuration);
            animationDuration.BindValueChanged(_ => resetAnimation());

            AddInternal(sprite = new Sprite()
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        public void Apply(LineLifetimeEntry entry)
        {
            Entry = entry;
            setLifetime(Entry.LifetimeStart, Entry.LifetimeEnd);
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Colour = Entry.Colour;
            Rotation = Entry.Rotation;
            sprite.Texture = textures.Get(Entry.GetLineTexturePath());

            resetAnimation();
        }

        protected override void FreeAfterUse()
        {
            Entry = null;
        }

        private void resetAnimation()
        {
            if (!IsInUse) return;
            ApplyTransformsAt(double.MinValue);
            ClearTransforms();
            using (BeginAbsoluteSequence(Entry.StartTime - Entry.AdjustedAnimationDuration))
                this.FadeIn(Entry.AdjustedAnimationDuration / 2).Then().ScaleTo(1, Entry.AdjustedAnimationDuration / 2).Then().FadeOut().Expire();
        }

        private void setLifetime(double lifetimeStart, double lifetimeEnd)
        {
            base.LifetimeStart = lifetimeStart;
            base.LifetimeEnd = lifetimeEnd;
            if (Entry != null)
            {
                Entry.LifetimeStart = lifetimeStart;
                Entry.LifetimeEnd = lifetimeEnd;
            }
        }
    }
}
