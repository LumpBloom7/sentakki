using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Types;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class SentakkiRotationHandler : SelectionRotationHandler
{
    [Resolved]
    private SentakkiSnapProvider snapProvider { get; set; } = null!;

    [Resolved]
    private HitObjectComposer composer { get; set; } = null!;

    [Resolved]
    private IEditorChangeHandler? changeHandler { get; set; }

    private BindableList<HitObject> selectedItems { get; } = new BindableList<HitObject>();

    private readonly Dictionary<HitObject, IOriginalState> originalPositions = [];

    [BackgroundDependencyLoader]
    private void load(EditorBeatmap editorBeatmap)
    {
        selectedItems.BindTo(editorBeatmap.SelectedHitObjects);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        selectedItems.BindCollectionChanged((_, __) => updateState());
    }

    private void updateState()
    {
        CanRotateAroundPlayfieldOrigin.Value = selectedItems.Count != 0;
        CanRotateAroundSelectionOrigin.Value = selectedItems.All(h => h is Touch or TouchHold) && selectedItems.Count > 1;

        var origin = GeometryUtils.MinimumEnclosingCircle(selectedItems.Where(h => h is IHasPosition)
                                                                       .Select(h => ((IHasPosition)h).Position)).Item1;
        DefaultOrigin = origin;
    }

    public override void Begin()
    {
        if (OperationInProgress.Value)
            throw new InvalidOperationException($"Cannot {nameof(Begin)} a rotate operation while another is in progress!");

        base.Begin();

        originalPositions.Clear();

        foreach (var item in selectedItems)
        {
            switch (item)
            {
                case IHasLane laned:
                    originalPositions[item] = new OriginalState<int>(laned.Lane);
                    break;

                case IHasPosition positional:
                    originalPositions[item] = new OriginalState<Vector2>(positional.Position);
                    break;
            }
        }

        changeHandler?.BeginChange();
    }

    public override void Update(float rotation, Vector2? origin = null)
    {
        int lanedRotationSteps = (int)MathF.Round(rotation / 45f);

        origin ??= GeometryUtils.MinimumEnclosingCircle(selectedItems.Where(h => h is IHasPosition)
                                                                     .Select(h => ((OriginalState<Vector2>)originalPositions[h]).Value)).Item1;

        // Let's rotate laned notes first
        foreach (var item in selectedItems)
        {
            if (!originalPositions.TryGetValue(item, out var originalState))
                continue;

            switch (item)
            {
                case SentakkiLanedHitObject lanedHitObject:
                    int originalLane = ((OriginalState<int>)originalState).Value;
                    int newLane = (originalLane + lanedRotationSteps).NormalizeLane();

                    if (lanedHitObject.Lane == newLane)
                        continue;
                    composer.Playfield.Remove(lanedHitObject);
                    lanedHitObject.Lane = newLane;
                    composer.Playfield.Add(lanedHitObject);
                    break;

                case IHasPosition:
                    Vector2 originalPosition = ((OriginalState<Vector2>)originalState).Value - origin.Value;
                    float originalAngle = Vector2.Zero.AngleTo(originalPosition);
                    float newAngle = originalAngle + rotation;

                    Vector2 newPosition = origin.Value + MathExtensions.PointOnCircle(originalPosition.Length, newAngle);

                    // If the snap provider is on, we assume they prefer to snap to a nearby snap point
                    if (snapProvider.TouchSnapGrid.Enabled)
                        newPosition = SentakkiBeatmapConverterOld.VALID_TOUCH_POSITIONS.MinBy(v => Vector2.DistanceSquared(v, newPosition));

                    switch (item)
                    {
                        case Touch touchNote:
                            touchNote.Position = newPosition;
                            break;

                        case TouchHold touchHold:
                            touchHold.Position = newPosition;
                            break;
                    }

                    break;
            }
        }
    }

    public override void Commit()
    {
        base.Commit();
        changeHandler?.EndChange();
        originalPositions.Clear();
    }

    private interface IOriginalState
    {
    }

    private readonly record struct OriginalState<T>(T Value) : IOriginalState;
}
