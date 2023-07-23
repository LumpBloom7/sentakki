using System;
using osuTK;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Input.Events;

namespace osu.Game.Rulesets.Sentakki.Edit.Toolbox;
public partial class ExpandableMenu<T> : CompositeDrawable, IExpandable, IHasCurrentValue<T>
                                            where T : struct, Enum
{
    public override bool HandlePositionalInput => true;

    public BindableBool Expanded { get; } = new BindableBool();

    public Bindable<T> Current { get; set; } = new Bindable<T>();

    private OsuDropdown<T> menu;

    private string expandedLabelText;
    private string unexpandedLabeltext = "";

    private OsuSpriteText label;

    public ExpandableMenu(string labelText)
    {
        expandedLabelText = labelText;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        InternalChild = new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Spacing = new Vector2(0f, 10f),
            Children = new Drawable[]
            {
                label = new OsuSpriteText(){
                    Text = labelText
                },
                menu = new NonBlockingDropdown
                {
                    RelativeSizeAxes = Axes.X,
                },
            }
        };
    }

    [Resolved]
    private IExpandingContainer? expandingContainer { get; set; }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        expandingContainer?.Expanded.BindValueChanged(containerExpanded =>
        {
            Expanded.Value = containerExpanded.NewValue;
        }, true);

        menu.Current = Current;
        Current.BindValueChanged(v =>
        {
            unexpandedLabeltext = $"{expandedLabelText}: {v.NewValue}";
            if (!Expanded.Value)
                label.Text = unexpandedLabeltext;
        }, true);

        Expanded.BindValueChanged(v =>
        {
            label.Text = v.NewValue ? expandedLabelText : unexpandedLabeltext;
            menu.FadeTo(v.NewValue ? 1f : 0f, 500, Easing.OutQuint);
            menu.BypassAutoSizeAxes = !v.NewValue ? Axes.Y : Axes.None;
        }, true);
    }

    private partial class NonBlockingDropdown : OsuEnumDropdown<T>
    {
        protected override DropdownMenu CreateMenu() => new NonBlockingMenu();

        private partial class NonBlockingMenu : OsuDropdownMenu
        {
            protected override bool OnHover(HoverEvent e)
            {
                base.OnHover(e);
                return false;
            }
        }
    }
}
