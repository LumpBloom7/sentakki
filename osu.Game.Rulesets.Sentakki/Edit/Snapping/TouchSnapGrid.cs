using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Snapping;

public partial class TouchPositionSnapGrid : VisibilityContainer
{
    public readonly Bindable<TernaryState> Enabled = new Bindable<TernaryState>(TernaryState.True);
    private Bindable<bool> activationRequested = new Bindable<bool>();

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        foreach (var position in SentakkiBeatmapConverterOld.VALID_TOUCH_POSITIONS)
        {
            AddInternal(new Circle
            {
                Position = position,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Colour = Colour4.White,
                Size = new Vector2(10)
            });
        }

        Enabled.ValueChanged += _ => updateVisibility();
        activationRequested.ValueChanged += _ => updateVisibility();
    }

    protected override void PopIn() => this.FadeIn(50);
    protected override void PopOut() => this.FadeOut(100);

    private void updateVisibility()
    {
        State.Value = (Enabled.Value is TernaryState.True && activationRequested.Value) ? Visibility.Visible : Visibility.Hidden;
    }

    public override void Show()
    {
        activationRequested.Value = true;
    }

    public override void Hide()
    {
        activationRequested.Value = false;
    }

    public Vector2 GetSnappedPosition(Vector2 original)
    {
        if (State.Value is Visibility.Hidden)
            return original;

        return SentakkiBeatmapConverterOld.VALID_TOUCH_POSITIONS.MinBy(v => Vector2.DistanceSquared(original, v));
    }
}