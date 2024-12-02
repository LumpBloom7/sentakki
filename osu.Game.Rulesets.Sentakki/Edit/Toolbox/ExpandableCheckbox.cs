using osuTK;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Rulesets.Sentakki.Edit.Toolbox;
public partial class ExpandableCheckbox : CompositeDrawable, IExpandable, IHasCurrentValue<bool>
{
    public override bool HandlePositionalInput => true;

    public BindableBool Expanded { get; } = new BindableBool();

    public Bindable<bool> Current { get; set; } = new Bindable<bool>();

    private string expandedLabelText;
    private string unexpandedLabeltext = "";

    private OsuSpriteText label;
    private OsuCheckbox checkbox;

    public ExpandableCheckbox(string labelText)
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
                checkbox = new OsuCheckbox(){
                    LabelText = labelText
                },
                label = new OsuSpriteText(){
                    Text = labelText
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

        checkbox.Current = Current;

        Current.BindValueChanged(v =>
        {
            unexpandedLabeltext = $"{expandedLabelText}: {v.NewValue}";
            label.Text = unexpandedLabeltext;
        }, true);

        Expanded.BindValueChanged(v =>
        {
            checkbox.FadeTo(v.NewValue ? 1f : 0f, 500, Easing.OutQuint);
            label.FadeTo(v.NewValue ? 0f : 1f, 500, Easing.OutQuint);
            checkbox.BypassAutoSizeAxes = !v.NewValue ? Axes.Y : Axes.None;
            label.BypassAutoSizeAxes = v.NewValue ? Axes.Y : Axes.None;
        }, true);
    }
}
