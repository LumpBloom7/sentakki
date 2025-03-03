using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.UI;
using Touch = osu.Game.Rulesets.Sentakki.Objects.Touch;

namespace osu.Game.Rulesets.Sentakki.UI
{
    // A special playfield specifically made for TouchNotes
    // Contains extra functionality to better propogate touch input to Touch notes, and avoids some double hit weirdness
    public partial class TouchPlayfield : Playfield
    {
        public TouchPlayfield()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
        }

        [Resolved]
        private DrawableSentakkiRuleset drawableSentakkiRuleset { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RegisterPool<Touch, DrawableTouch>(16);
            RegisterPool<ScorePaddingObject, DrawableScorePaddingObject>(20);
        }

        protected override HitObjectLifetimeEntry CreateLifetimeEntry(HitObject hitObject) => new SentakkiHitObjectLifetimeEntry(hitObject, drawableSentakkiRuleset);
    }
}
