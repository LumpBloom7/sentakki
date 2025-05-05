using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides.Visualiser;

public partial class EditorSlidePartPiece : SmoothPath, IHasTooltip, IHasContextMenu
{
    private readonly Slide slide;
    private readonly SlideBodyInfo slideBodyInfo;
    private readonly int index = 0;

    private SlideBodyPart part => slideBodyInfo.SlidePathParts[index];

    [Resolved(CanBeNull = true)]
    private EditorBeatmap? editorBeatmap { get; set; }

    [Resolved]
    private SlidePathVisualiser slidePathVisualiser { get; set; } = null!;

    public EditorSlidePartPiece(Slide slide, SlideBodyInfo info, int index)
    {
        this.slide = slide;
        slideBodyInfo = info;
        this.index = index;

        updateContextMenuItems();
    }

    public LocalisableString TooltipText
    {
        get
        {
            StringBuilder builder = new();

            builder.AppendLine($"Shape: {part.Shape}");

            if (part.Shape is not SlidePaths.PathShapes.Fan)
                builder.AppendLine($"Mirrored: {part.Mirrored}");

            if (part.Shape is not SlidePaths.PathShapes.Fan or SlidePaths.PathShapes.Thunder)
                builder.AppendLine($"End Offset: {part.EndOffset}");

            return builder.ToString().Trim();
        }
    }

    public Color4 AccentColour { get; set; }

    public MenuItem[]? ContextMenuItems { get; private set; }

    private static readonly int[] validEndOffsets = [0, 1, 2, 3, 4, 5, 6, 7];

    protected override void LoadComplete()
    {
        base.LoadComplete();
        Colour = AccentColour;
    }

    protected override bool OnHover(HoverEvent e)
    {
        Colour = AccentColour.LightenHSL(0.5f);
        return false;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        Colour = AccentColour;
        base.OnHoverLost(e);
    }

    private void updateContextMenuItems()
    {
        List<MenuItem> contextMenuItems = [];
        contextMenuItems.Add(new OsuMenuItem("Shape", MenuItemType.Standard)
        {
            Items = Enum.GetValues<SlidePaths.PathShapes>()
                        .Select(s =>
                        {
                            var menuItem = new TernaryStateRadioMenuItem($"{s}", action: _ => changeShape(s));
                            menuItem.State.Value = part.Shape == s ? TernaryState.True : TernaryState.False;

                            return menuItem;
                        }).ToArray()
        });

        contextMenuItems.Add(new OsuMenuItem("End offset", MenuItemType.Standard)
        {
            Items = validEndOffsets
                        .Where(o => SlidePaths.CheckSlideValidity(part with { EndOffset = o }))
                        .Select(s =>
                        {
                            var menuItem = new TernaryStateRadioMenuItem($"{s}", action: _ => changeEndOffset(s));
                            menuItem.State.Value = part.EndOffset.NormalizePath() == s ? TernaryState.True : TernaryState.False;

                            return menuItem;
                        }).ToArray()
        });

        var mirroredTernaryMenuItem = new TernaryStateToggleMenuItem("Mirrored", MenuItemType.Standard, t => changeMirrored(t is TernaryState.True));

        mirroredTernaryMenuItem.State.Value = part.Mirrored ? TernaryState.True : TernaryState.False;

        contextMenuItems.Add(mirroredTernaryMenuItem);

        contextMenuItems.Add(new OsuMenuItem("Add segment", MenuItemType.Standard, addSegment));
        contextMenuItems.Add(new OsuMenuItem("Delete segment", MenuItemType.Destructive, slideBodyInfo.SlidePathParts.Length <= 1 ? null : deleteSegment));

        ContextMenuItems = [.. contextMenuItems];
    }

    private void changeShape(SlidePaths.PathShapes shape)
    {
        if (shape == part.Shape)
            return;

        for (int i = 0; i < 8; ++i)
        {
            int direction = (i % 2 == 0) ? -1 : 1;
            int changeAmount = (int)Math.Ceiling(i / 2f);

            int targetOffset = part.EndOffset + (changeAmount * direction);

            SlideBodyPart newPart = new SlideBodyPart(shape, targetOffset, part.Mirrored);

            if (SlidePaths.CheckSlideValidity(newPart))
            {
                SlideBodyPart[] newList = [.. slideBodyInfo.SlidePathParts];
                newList[index] = newPart;

                submitChange(newList);
                break;
            }
        }
    }

    private void changeEndOffset(int endOffset)
    {
        if (endOffset == part.EndOffset)
            return;

        SlideBodyPart[] newList = [.. slideBodyInfo.SlidePathParts];
        newList[index] = new SlideBodyPart(part.Shape, endOffset, part.Mirrored);
        submitChange(newList);
    }

    private void changeMirrored(bool mirrored)
    {
        if (mirrored == part.Mirrored)
            return;

        SlideBodyPart[] newList = [.. slideBodyInfo.SlidePathParts];
        newList[index] = new SlideBodyPart(part.Shape, part.EndOffset, mirrored);

        submitChange(newList);
    }

    private void addSegment()
    {
        List<SlideBodyPart> newList = [.. slideBodyInfo.SlidePathParts];
        newList.Insert(index, part);

        submitChange([.. newList]);
    }

    private void deleteSegment()
    {
        List<SlideBodyPart> newList = [.. slideBodyInfo.SlidePathParts];
        newList.RemoveAt(index);
        submitChange([.. newList]);
    }

    private void submitChange(SlideBodyPart[] parts)
    {
        editorBeatmap?.BeginChange();
        slideBodyInfo.SlidePathParts = parts;
        editorBeatmap?.Update(slide);
        editorBeatmap?.EndChange();

        slidePathVisualiser.ReloadVisualiser();
    }
}
