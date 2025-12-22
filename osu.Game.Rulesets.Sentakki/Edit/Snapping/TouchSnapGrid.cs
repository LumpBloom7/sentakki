using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Snapping;

public partial class TouchPositionSnapGrid : VisibilityContainer
{
    protected override bool StartHidden => true;

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
    }

    protected override void PopIn() => this.FadeIn(50);
    protected override void PopOut() => this.FadeOut(100);

    public Vector2 GetSnappedPosition(Vector2 original)
    {
        if (State.Value is Visibility.Hidden)
            return original;

        return SentakkiBeatmapConverterOld.VALID_TOUCH_POSITIONS.MinBy(v => Vector2.DistanceSquared(original, v));
    }
}