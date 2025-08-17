using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Snapping;

public partial class SentakkiTouchSnapGrid : CompositeDrawable
{
    private readonly Bindable<TernaryState> enabled = new Bindable<TernaryState>(TernaryState.True);

    public bool Enabled => enabled.Value == TernaryState.True;

    public DrawableTernaryButton CreateTernaryButton() => new DrawableTernaryButton
    {
        Current = enabled,
        Description = "Touch Snap",
        CreateIcon = () => new SpriteIcon { Icon = FontAwesome.Solid.Thumbtack }
    };

    private Container dotContainer = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Origin = Anchor.Centre;
        AddInternal(dotContainer = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        });

        foreach (var point in SentakkiBeatmapConverterOld.VALID_TOUCH_POSITIONS)
        {
            dotContainer.Add(new Circle
            {
                Size = new Vector2(10),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Position = point
            });
        }
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        enabled.BindValueChanged(toggleVisibility);
    }

    private void toggleVisibility(ValueChangedEvent<TernaryState> v)
    {
        if (v.NewValue is TernaryState.True)
        {
            dotContainer.Show();
            return;
        }

        dotContainer.Hide();
    }

    public SnapResult GetSnapResult(Vector2 screenSpacePosition)
    {
        if (enabled.Value is not TernaryState.True)
            return new SnapResult(screenSpacePosition, null);

        var localPosition = ToLocalSpace(screenSpacePosition);

        var closestPoint = SentakkiBeatmapConverterOld.VALID_TOUCH_POSITIONS.MinBy(v => Vector2.DistanceSquared(v, localPosition));

        return new SnapResult(ToScreenSpace(closestPoint), null);
    }
}
