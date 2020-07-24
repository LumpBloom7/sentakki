using osu.Game.Rulesets.Scoring;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Skinning;
using osu.Game.Audio;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideFirstNode : DrawableSlideNode
    {
        [Resolved(canBeNull: true)]
        private GameplayClock gameplayClock { get; set; }

        private readonly Bindable<bool> userPositionalHitSounds = new Bindable<bool>(false);
        private readonly SkinnableSound slideSound;
        public DrawableSlideFirstNode(Slide.SlideNode node, DrawableSlide slideNote)
            : base(node, slideNote)
        {
            AddInternal(slideSound = new SkinnableSound(new SampleInfo("slide")));
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager osuConfig, SentakkiRulesetConfigManager sentakkiConfig)
        {
            osuConfig.BindWith(OsuSetting.PositionalHitSounds, userPositionalHitSounds);
        }

        public override void PlaySamples()
        {
            base.PlaySamples();
            if (slideSound != null && Result.Type == HitResult.Perfect && (!gameplayClock?.IsSeeking ?? false))
            {
                const float balance_adjust_amount = 0.4f;
                slideSound.Balance.Value = balance_adjust_amount * (userPositionalHitSounds.Value ? SamplePlaybackPosition - 0.5f : 0);
                slideSound.Play();
            }
        }
    }
}
