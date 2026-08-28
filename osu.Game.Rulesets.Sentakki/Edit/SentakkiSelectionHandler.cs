using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Sentakki.Edit;

[Cached]
public partial class SentakkiSelectionHandler : EditorSelectionHandler
{
    [Resolved]
    private SentakkiHitObjectComposer composer { get; set; } = null!;

    public SentakkiSelectionHandler()
    {
        Origin = Anchor.Centre;
        Anchor = Anchor.Centre;
    }

    // public override SelectionRotationHandler CreateRotationHandler() => new SentakkiRotationHandler();

    protected override void OnSelectionChanged()
    {
        base.OnSelectionChanged();

        // We are always able to flip hitobjects
        SelectionBox.CanFlipX = true;
        SelectionBox.CanFlipY = true;
        // SelectionBox.CanReverse = SelectedItems.Count > 1;
    }

    public override bool HandleReverse()
    {
        var orderedItems = SelectedItems.OrderBy(s => s.GetEndTime()).ToArray();

        List<double> times = [];

        foreach (var item in orderedItems)
        {
            times.Add(item.StartTime);

            if (item is IHasDuration d)
                times.Add(d.EndTime);
        }

        times.Sort();
        times.Reverse();

        int i = 0;

        foreach (var item in orderedItems)
        {
            if (item is IHasDuration d)
            {
                double et = times[i++];
                double st = times[i++];

                item.StartTime = st;
                d.Duration = et - st;
            }
            else
            {
                item.StartTime = times[i++];
            }

            EditorBeatmap.Update(item);
        }

        return base.HandleReverse();
    }

    public override bool HandleRotation(float angle)
    {
        return base.HandleRotation(angle);
    }

    public override bool HandleFlip(Direction direction, bool flipOverOrigin)
    {
        return direction switch
        {
            Direction.Horizontal => flipHorizontally(),
            Direction.Vertical => flipVertically(),
            _ => false
        };
    }

    private bool flipHorizontally()
    {
        if (SelectedItems.Count == 0)
            return false;

        EditorBeatmap.BeginChange();

        foreach (var item in SelectedItems)
        {
            switch (item)
            {
                case SentakkiLanedHitObject laned:
                    composer.Playfield.Remove(laned);
                    laned.Lane = 7 - laned.Lane;

                    if (laned is Slide s)
                    {
                        foreach (var slideInfo in s.SlideInfoList)
                        {
                            slideInfo.Segments =
                            [
                                ..slideInfo.Segments.Select(s => s with
                                {
                                    RelativeEndLane = (-s.RelativeEndLane).NormalizeLane(),
                                    Mirrored = !s.Mirrored
                                })
                            ];
                        }
                    }
                    EditorBeatmap.Update(laned);
                    composer.Playfield.Add(laned);
                    break;

                case IHasPosition position:
                    position.X = -position.X;
                    EditorBeatmap.Update(item);
                    break;
            }
        }

        EditorBeatmap.EndChange();

        return true;
    }

    private bool flipVertically()
    {
        if (SelectedItems.Count == 0)
            return false;

        EditorBeatmap.BeginChange();

        foreach (var item in SelectedItems)
        {
            switch (item)
            {
                case SentakkiLanedHitObject laned:
                    composer.Playfield.Remove(laned);
                    laned.Lane = (3 - laned.Lane).NormalizeLane();

                    if (laned is Slide s)
                    {
                        foreach (var slideInfo in s.SlideInfoList)
                        {
                            slideInfo.Segments =
                            [
                                ..slideInfo.Segments.Select(s => s with
                                {
                                    RelativeEndLane = (-s.RelativeEndLane).NormalizeLane(),
                                    Mirrored = !s.Mirrored
                                })
                            ];
                        }
                    }

                    EditorBeatmap.Update(laned);
                    composer.Playfield.Add(laned);
                    break;

                case IHasPosition position:
                    position.Y = -position.Y;
                    EditorBeatmap.Update(item);
                    break;
            }
        }

        EditorBeatmap.EndChange();

        return true;
    }

    #region ContextMenu

    public IEnumerable<MenuItem> GetContextMenuItemsForSelection() => GetContextMenuItemsForSelection(SelectedBlueprints);

    protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint<HitObject>> selection)
    {
        foreach (var item in base.GetContextMenuItemsForSelection(selection))
            yield return item;

        yield return new OsuMenuItemSpacer();

        var items = selection.Select(s => s.Item).OfType<SentakkiHitObject>();

        yield return new OsuMenuItem("Modifiers")
        {
            Items =
            [
                new TernaryStateToggleMenuItem("Break")
                {
                    State = { BindTarget = composer.BreakTernaryState },
                    Hotkey = new Hotkey(new KeyCombination(InputKey.R))
                },
                new TernaryStateToggleMenuItem("EX ")
                {
                    State = { BindTarget = composer.ExTernaryState, },
                    Hotkey = new Hotkey(new KeyCombination(InputKey.T))
                }
            ]
        };

        var slideBodies = items.OfType<Slide>().SelectMany(s => s.SlideInfoList);

        if (!slideBodies.Any()) yield break;

        yield return new OsuMenuItem("Slide Modifiers")
        {
            Items =
            [
                new TernaryStateToggleMenuItem("Break") { State = { BindTarget = composer.BreakSlideTernaryState } },
                new TernaryStateToggleMenuItem("EX") { State = { BindTarget = composer.ExSlideTernaryState } }
            ]
        };

        yield return new TernaryStateToggleMenuItem("Omit slide tap") { State = { BindTarget = composer.OmitSlideTapTernaryState } };
    }

    #endregion
}
