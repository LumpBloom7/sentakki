using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
        ExTernaryState.ValueChanged += v => applyTernaryChanges<SentakkiHitObject>(setExState, v.NewValue);
        BreakTernaryState.ValueChanged += v => applyTernaryChanges<SentakkiHitObject>(setBreakState, v.NewValue);

        exSlideTernaryState.ValueChanged += v => applyTernaryChanges<Slide>(setExSlideState, v.NewValue);
        breakSlideTernaryState.ValueChanged += v => applyTernaryChanges<Slide>(setBreakSlideState, v.NewValue);
    }

    public override SelectionRotationHandler CreateRotationHandler() => new SentakkiRotationHandler();

    protected override void OnSelectionChanged()
    {
        base.OnSelectionChanged();

        // We are always able to flip hitobjects
        SelectionBox.CanFlipX = true;
        SelectionBox.CanFlipY = true;
        SelectionBox.CanReverse = SelectedItems.Count > 1;
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
                    State = { BindTarget = BreakTernaryState },
                    Hotkey = new Hotkey(new KeyCombination(InputKey.R))
                },
                new TernaryStateToggleMenuItem("EX ")
                {
                    State = { BindTarget = ExTernaryState, },
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
                new TernaryStateToggleMenuItem("Break") { State = { BindTarget = breakSlideTernaryState } },
                new TernaryStateToggleMenuItem("EX") { State = { BindTarget = exSlideTernaryState } }
            ]
        };
    }

    public readonly Bindable<TernaryState> ExTernaryState = new Bindable<TernaryState>();
    public readonly Bindable<TernaryState> BreakTernaryState = new Bindable<TernaryState>();

    private readonly Bindable<TernaryState> exSlideTernaryState = new Bindable<TernaryState>();
    private readonly Bindable<TernaryState> breakSlideTernaryState = new Bindable<TernaryState>();

    protected override void UpdateTernaryStates()
    {
        var selectedItems = SelectedItems.OfType<SentakkiHitObject>();
        ExTernaryState.Value = GetStateFromSelection(selectedItems, h => h.Ex);
        BreakTernaryState.Value = GetStateFromSelection(selectedItems, h => h.Break);

        var selectedSlideBodies = selectedItems.OfType<Slide>().SelectMany(s => s.SlideInfoList);
        breakSlideTernaryState.Value = GetStateFromSelection(selectedSlideBodies, s => s.Break);
    }

    private void applyTernaryChanges<T>(Action<T, bool> applicator, TernaryState newTernaryState) where T : HitObject
    {
        // We can get into an indeterminate state when mixing notes with different break/ex values
        // We don't want to force enable/disable from this intermediate state
        if (newTernaryState is TernaryState.Indeterminate)
            return;

        var selectedItems = SelectedItems.OfType<T>();

        bool newValue = newTernaryState is TernaryState.True;

        EditorBeatmap.BeginChange();

        foreach (var item in selectedItems)
        {
            applicator(item, newValue);
            EditorBeatmap.Update(item);
        }

        EditorBeatmap.EndChange();
    }

    private void setExState(SentakkiHitObject hitObject, bool newValue) => hitObject.Ex = newValue;
    private void setBreakState(SentakkiHitObject hitObject, bool newValue) => hitObject.Break = newValue;

    private void setExSlideState(Slide slide, bool newValue) => slide.SlideInfoList.ForEach(si => si.Ex = newValue);
    private void setBreakSlideState(Slide slide, bool newValue) => slide.SlideInfoList.ForEach(si => si.Break = newValue);

    #endregion
}
