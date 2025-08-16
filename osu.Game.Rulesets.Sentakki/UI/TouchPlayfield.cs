using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sentakki.UI;

public partial class TouchPlayfield : Playfield
{
    public TouchPlayfield()
    {
        RelativeSizeAxes = Axes.Both;
        Anchor = Origin = Anchor.Centre;
    }

    [Resolved]
    private DrawableSentakkiRuleset drawableSentakkiRuleset { get; set; } = null!;

    protected override HitObjectLifetimeEntry CreateLifetimeEntry(HitObject hitObject) => new SentakkiHitObjectLifetimeEntry(hitObject, drawableSentakkiRuleset);

    [BackgroundDependencyLoader]
    private void load()
    {
        RegisterPool<Touch, DrawableTouch>(8);
        RegisterPool<ScorePaddingObject, DrawableScorePaddingObject>(32);
    }
}
