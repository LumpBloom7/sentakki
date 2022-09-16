using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Sentakki.Configuration;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides
{
    public abstract class SlideVisualBase<T> : CompositeDrawable, ISlideVisual where T : Drawable
    {
        // This will be proxied, so a must.
        public override bool RemoveWhenNotAlive => false;

        private double progress;
        public double Progress
        {
            get => progress;
            set
            {
                progress = value;
                UpdateProgress();
            }
        }

        public SlideVisualBase()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        protected Container<T> Chevrons;

        private readonly BindableBool snakingIn = new BindableBool(true);

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfig)
        {
            sentakkiConfig?.BindWith(SentakkiRulesetSettings.SnakingSlideBody, snakingIn);

            AddRangeInternal(new Drawable[]{
                Chevrons = new Container<T>(),
            });
        }

        protected void UpdateProgress()
        {
            for (int i = 0; i < Chevrons.Count; i++)
            {
                ISlideChevron.UpdateProgress((ISlideChevron)Chevrons[i], progress);
            }
        }

        public void PerformEntryAnimation(double duration)
        {
            if (snakingIn.Value)
            {
                double fadeDuration = duration / Chevrons.Count;
                double currentOffset = duration / 2;
                for (int j = Chevrons.Count - 1; j >= 0; j--)
                {
                    var chevron = Chevrons[j];
                    chevron.FadeOut().Delay(currentOffset).FadeIn(fadeDuration * 2).Finally(finalSteps);
                    currentOffset += fadeDuration / 2;
                }
            }
            else
            {
                Chevrons.FadeOut().Delay(duration / 2).FadeIn(duration / 2);
            }

            void finalSteps(T chevron) => ISlideChevron.UpdateProgress((ISlideChevron)chevron, progress);
        }

        public void PerformExitAnimation(double duration)
        {
            int chevronsLeft = Chevrons.Count(c => c.Alpha != 0);
            double fadeDuration = duration / chevronsLeft;
            double currentOffset = 0;
            for (int j = Chevrons.Count - 1; j >= 0; j--)
            {
                var chevron = Chevrons[j];
                if (chevron.Alpha == 0)
                {
                    continue;
                }
                chevron.Delay(currentOffset).FadeOut(fadeDuration * 2);
                currentOffset += fadeDuration / 2;
            }
        }

        // For cases where pooled objects need to be freed
        public virtual void Free()
        {
        }
    }
}
