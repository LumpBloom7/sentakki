using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets.Sentakki.Extensions;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI.Components.HitObjectLine;

public partial class DrawableLine : PoolableDrawable
{
    public override bool RemoveCompletedTransforms => false;

    public LineLifetimeEntry Entry = null!;

    private CircularProgress line = null!;

    public DrawableLine()
    {
        RelativeSizeAxes = Axes.Both;
        Anchor = Origin = Anchor.Centre;
        Scale = new Vector2(.224f);
        Alpha = 0;
    }

    private readonly BindableDouble animationDuration = new BindableDouble(1000);

    [BackgroundDependencyLoader]
    private void load(DrawableSentakkiRuleset? drawableRuleset)
    {
        animationDuration.TryBindTo(drawableRuleset?.AdjustedAnimDuration);
        animationDuration.BindValueChanged(_ => resetAnimation());

        AddInternal(line = new CircularProgress
        {
            RelativeSizeAxes = Axes.Both,
            FillMode = FillMode.Fit,
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            InnerRadius = 0.026f,
            RoundedCaps = true,
            Alpha = 0.8f,
            Progress = 0,
        });
    }

    protected override void PrepareForUse()
    {
        base.PrepareForUse();

        Colour = Entry.Colour;
        resetAnimation();
    }

    private void resetAnimation()
    {
        if (!IsInUse) return;

        ApplyTransformsAt(double.MinValue, true);
        ClearTransforms(true);

        using (BeginAbsoluteSequence(Entry.StartTime - animationDuration.Value))
        {
            double entryDuration = animationDuration.Value / 2;
            line.TransformTo(nameof(line.Progress), (double)Entry.AngleRange, entryDuration);
            this.RotateTo(Entry.Rotation + Entry.AngleRange * 180).Then().RotateTo(Entry.Rotation, entryDuration);
            this.FadeIn(entryDuration).Then().ScaleTo(1, entryDuration).Then().FadeOut();
        }
    }
}
