using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Statistics
{
    public partial class JudgementChart : FillFlowContainer
    {
        private const double entry_animation_duration = 150;

        // The list of entries that we should create, placed here to reduce dupe code
        private static readonly (string, Func<HitEvent, bool>)[] entries =
        {
            ("Tap", e => e.HitObject is Tap x && !x.Break),
            ("Hold", e => ((e.HitObject is Hold.HoldHead) && !((SentakkiLanedHitObject)e.HitObject).Break) || e.HitObject is Hold),
            ("Slide", e => e.HitObject is SlideBody x && !x.Break),
            ("Touch", e => e.HitObject is Touch),
            ("Touch Hold", e => e.HitObject is TouchHold),
            // Note Hold and Slide breaks are applied to child objects, not itself.
            ("Break", e => e.HitObject is SentakkiLanedHitObject x && (x is not Hold) && (x is not Slide) && x.Break),
        };

        private readonly IReadOnlyList<HitEvent> hitEvents;

        public JudgementChart(IReadOnlyList<HitEvent> hitEvents)
        {
            this.hitEvents = hitEvents;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Origin = Anchor.Centre;
            Size = new Vector2(1, 250);
            RelativeSizeAxes = Axes.X;

            foreach (var (name, predicate) in entries)
                AddInternal(new ChartEntry(name, hitEvents.Where(predicate).ToList()));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            double delay = 0;
            foreach (ChartEntry child in Children)
            {
                using (BeginDelayedSequence(delay, true))
                    child.AnimateEntry(entry_animation_duration);
                delay += entry_animation_duration;
            }
        }
    }
}
