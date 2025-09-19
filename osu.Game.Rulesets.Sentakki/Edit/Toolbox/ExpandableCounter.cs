using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using System;
using System.Globalization;
using System.Numerics;
using Vector2 = osuTK.Vector2;

namespace osu.Game.Rulesets.Sentakki.Edit.Toolbox;

public partial class ExpandableCounter<T> : CompositeDrawable, IExpandable, IHasCurrentValue<T>
    where T : struct, IComparable<T>, IConvertible, IEquatable<T>, INumber<T>, IMinMaxValue<T>
{
    public override bool HandlePositionalInput => true;

    public BindableBool Expanded { get; } = new BindableBool();

    public Bindable<T> Current { get; set; } = new BindableNumber<T>(default);

    private readonly OsuSpriteText label;
    private readonly OsuSpriteText counter;

    private readonly string labelText;
    private string unexpandedLabeltext = "";
    private readonly GridContainer grid;

    private readonly string labelFormat;

    public ExpandableCounter(string label, string format = @"{0:0.##}")
    {
        labelFormat = format;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        labelText = label;

        InternalChild = new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Spacing = new Vector2(0f, 10f),
            Children =
            [
                this.label = new OsuSpriteText
                {
                    Text = label
                },
                grid = new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                    ColumnDimensions = [new Dimension(GridSizeMode.Distributed), new Dimension(GridSizeMode.Distributed), new Dimension(GridSizeMode.Distributed)],
                    Content = new Drawable[][]
                    {
                        [
                            new IconButton
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 1,
                                Icon = FontAwesome.Solid.ChevronLeft,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Action = OnLeftButtonPressed
                            },
                            counter = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                            new IconButton
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 1,
                                Icon = FontAwesome.Solid.ChevronRight,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Action = OnRightButtonPressed
                            }
                        ]
                    }
                }
            ]
        };
    }

    [Resolved]
    private IExpandingContainer? expandingContainer { get; set; }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        Current.BindValueChanged(v =>
        {
            counter.Text = string.Format(labelFormat, v.NewValue.ToSingle(NumberFormatInfo.InvariantInfo));
            unexpandedLabeltext = $"{labelText}: {counter.Text}";
            if (!Expanded.Value)
                label.Text = unexpandedLabeltext;
        }, true);

        expandingContainer?.Expanded.BindValueChanged(containerExpanded =>
        {
            Expanded.Value = containerExpanded.NewValue;
        }, true);

        Expanded.BindValueChanged(v =>
        {
            label.Text = v.NewValue ? labelText : unexpandedLabeltext;
            grid.FadeTo(v.NewValue ? 1f : 0f);
            grid.BypassAutoSizeAxes = !v.NewValue ? Axes.Y : Axes.None;
        }, true);
    }

    protected virtual void OnLeftButtonPressed() { }
    protected virtual void OnRightButtonPressed() { }
}
