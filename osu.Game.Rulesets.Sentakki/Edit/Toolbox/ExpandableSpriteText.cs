using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Rulesets.Sentakki.Edit.Toolbox;

public partial class ExpandableSpriteText : TextFlowContainer, IExpandable
{
    public BindableBool Expanded { get; } = new BindableBool();

    public string ExpandedLabel { get; set; } = "";

    [Resolved]
    private IExpandingContainer? expandingContainer { get; set; }

    public ExpandableSpriteText()
    {
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        expandingContainer?.Expanded.BindValueChanged(containerExpanded =>
        {
            Expanded.Value = containerExpanded.NewValue;
        }, true);

        Expanded.BindValueChanged(v =>
        {
            Text = v.NewValue ? ExpandedLabel : "";
            BypassAutoSizeAxes = !v.NewValue ? Axes.Y : Axes.None;
            this.FadeTo(v.NewValue ? 1f : 0f);
        }, true);
    }

    protected override SpriteText CreateSpriteText()
    {
        return new OsuSpriteText()
        {
            Font = OsuFont.Torus.With(size: 12),
        };
    }
}
