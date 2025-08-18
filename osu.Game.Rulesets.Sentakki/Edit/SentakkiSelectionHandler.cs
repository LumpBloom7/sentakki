using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class SentakkiSelectionHandler : EditorSelectionHandler
{
    public readonly Bindable<TernaryState> SelectionBreakState = new Bindable<TernaryState>();
    public readonly Bindable<TernaryState> SelectionExState = new Bindable<TernaryState>();

    public readonly Bindable<TernaryState> SelectionSlideNoTapState = new Bindable<TernaryState>();

    private readonly Bindable<TernaryState> selectionSlideBodyBreakState = new Bindable<TernaryState>();
    private readonly Bindable<TernaryState> selectionSlideBodyExState = new Bindable<TernaryState>();

    [Resolved]
    private SentakkiHitObjectComposer composer { get; set; } = null!;

    public override SelectionRotationHandler CreateRotationHandler() => new SentakkiRotationHandler();

    protected override void OnSelectionChanged()
    {
        base.OnSelectionChanged();

        SelectionBox.CanFlipX = true;
        SelectionBox.CanFlipY = true;
    }

    public override bool HandleFlip(Direction direction, bool flipOverOrigin)
    {
        var sentakkiPlayfield = composer.Playfield;

        var selectedObjects = EditorBeatmap.SelectedHitObjects.ToArray();

        EditorBeatmap.PerformOnSelection(ho =>
        {
            switch (ho)
            {
                case SentakkiLanedHitObject laned:
                    sentakkiPlayfield.Remove(laned);

                    switch (direction)
                    {
                        case Direction.Horizontal:
                            laned.Lane = 7 - laned.Lane;
                            break;

                        case Direction.Vertical:
                        {
                            laned.Lane = (3 - laned.Lane) % 8;
                            if (laned.Lane < 0) laned.Lane += 8;
                            break;
                        }
                    }

                    if (laned is Slide slide)
                    {
                        foreach (var slideInfo in slide.SlideInfoList)
                        {
                            for (int i = 0; i < slideInfo.SlidePathParts.Length; ++i)
                            {
                                ref var part = ref slideInfo.SlidePathParts[i];
                                part.EndOffset = (part.EndOffset * -1).NormalizeLane();
                                part.Mirrored = !part.Mirrored;
                            }

                            slideInfo.UpdatePaths();
                        }
                    }

                    sentakkiPlayfield.Add(laned);
                    break;

                case IHasPosition touch:
                    Vector2 newPosition = touch.Position;

                    switch (direction)
                    {
                        case Direction.Horizontal:
                            newPosition.X = -touch.Position.X;
                            break;

                        case Direction.Vertical:
                            newPosition.Y = -touch.Position.Y;
                            break;
                    }

                    touch.Position = newPosition;
                    break;
            }
        });

        EditorBeatmap.SelectedHitObjects.Clear();
        EditorBeatmap.SelectedHitObjects.AddRange(selectedObjects);

        return SelectedItems.Any(a => a is not TouchHold);
    }

    public SentakkiSelectionHandler()
    {
        SelectionBreakState.ValueChanged += s =>
        {
            switch (s.NewValue)
            {
                case TernaryState.True:
                case TernaryState.False:
                    setBreakState(s.NewValue == TernaryState.True);
                    return;
            }
        };

        SelectionExState.ValueChanged += s =>
        {
            switch (s.NewValue)
            {
                case TernaryState.True:
                case TernaryState.False:
                    setExState(s.NewValue == TernaryState.True);
                    return;
            }
        };

        selectionSlideBodyBreakState.ValueChanged += s =>
        {
            switch (s.NewValue)
            {
                case TernaryState.True:
                case TernaryState.False:
                    setSlideBodyBreakState(s.NewValue == TernaryState.True);
                    return;
            }
        };

        selectionSlideBodyExState.ValueChanged += s =>
        {
            switch (s.NewValue)
            {
                case TernaryState.True:
                case TernaryState.False:
                    setSlideBodyExState(s.NewValue == TernaryState.True);
                    return;
            }
        };

        SelectionSlideNoTapState.ValueChanged += s =>
        {
            switch (s.NewValue)
            {
                case TernaryState.True:
                case TernaryState.False:
                    setSlideNoTapState(s.NewValue == TernaryState.True);
                    return;
            }
        };
    }

    public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent)
    {
        if (SelectedBlueprints.All(bp => bp.Item is SentakkiLanedHitObject))
            return true;

        if (!SelectedBlueprints.All(bp => bp.Item is IHasPosition)) return false;

        // Special movement handling to ensure that all touch notes are within 250 units from the playfield centre
        moveTouchNotes(this.ScreenSpaceDeltaToParentSpace(moveEvent.ScreenSpaceDelta));
        return true;
    }

    private void setBreakState(bool state)
    {
        var lhos = EditorBeatmap.SelectedHitObjects.OfType<SentakkiHitObject>();

        EditorBeatmap.BeginChange();

        foreach (var lho in lhos)
        {
            if (lho.Break == state)
                continue;

            lho.Break = state;
            EditorBeatmap.Update(lho);
        }

        EditorBeatmap.EndChange();
    }

    private void setSlideBodyBreakState(bool state)
    {
        var lhos = EditorBeatmap.SelectedHitObjects.OfType<Slide>();

        EditorBeatmap.BeginChange();

        foreach (var slide in lhos)
        {
            bool adjusted = false;

            foreach (var body in slide.SlideInfoList)
            {
                if (body.Break == state)
                    continue;

                body.Break = state;
                adjusted = true;
            }

            if (adjusted)
                EditorBeatmap.Update(slide);
        }

        EditorBeatmap.EndChange();
    }

    private void setSlideBodyExState(bool state)
    {
        var lhos = EditorBeatmap.SelectedHitObjects.OfType<Slide>();

        EditorBeatmap.BeginChange();

        foreach (var slide in lhos)
        {
            bool adjusted = false;

            foreach (var body in slide.SlideInfoList)
            {
                if (body.Ex == state)
                    continue;

                body.Ex = state;
                adjusted = true;
            }

            if (adjusted)
                EditorBeatmap.Update(slide);
        }

        EditorBeatmap.EndChange();
    }

    private void setSlideNoTapState(bool state)
    {
        var sentakkiPlayfield = composer.Playfield;

        var selectedObjects = EditorBeatmap.SelectedHitObjects.ToArray();

        var lhos = EditorBeatmap.SelectedHitObjects.OfType<Slide>();

        EditorBeatmap.BeginChange();

        foreach (var slide in lhos)
        {
            var newState = state ? Slide.TapTypeEnum.None : Slide.TapTypeEnum.Star;

            if (slide.TapType == newState)
                continue;

            slide.TapType = newState;
            EditorBeatmap.Update(slide);

            // Revalidates the hit object lines
            sentakkiPlayfield.Remove(slide);
            sentakkiPlayfield.Add(slide);
        }

        EditorBeatmap.EndChange();

        SelectedItems.Clear();
        SelectedItems.AddRange(selectedObjects);
    }

    private void setExState(bool state)
    {
        var shos = EditorBeatmap.SelectedHitObjects.OfType<SentakkiHitObject>().Where(s => s is not TouchHold);

        EditorBeatmap.BeginChange();

        foreach (var sho in shos)
        {
            if (sho.Ex == state)
                continue;

            sho.Ex = state;
            EditorBeatmap.Update(sho);
        }

        EditorBeatmap.EndChange();
    }

    protected override void UpdateTernaryStates()
    {
        base.UpdateTernaryStates();

        SelectionBreakState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<SentakkiHitObject>(), h => h.Break);

        SelectionExState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<SentakkiHitObject>().Where(s => s is not TouchHold), s => s.Ex);

        selectionSlideBodyBreakState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<Slide>().SelectMany(h => h.SlideInfoList), s => s.Break);
        selectionSlideBodyExState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<Slide>().SelectMany(h => h.SlideInfoList), s => s.Ex);

        SelectionSlideNoTapState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<Slide>(), s => s.TapType is Slide.TapTypeEnum.None);
    }

    protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint<HitObject>> selection)
    {
        if (selection.Any(s => s.Item is not Slide sho || sho.TapType is not Slide.TapTypeEnum.None))
        {
            yield return new TernaryStateToggleMenuItem("Break") { State = { BindTarget = SelectionBreakState } };

            if (selection.Any(s => s.Item is not TouchHold))
                yield return new TernaryStateToggleMenuItem("Ex") { State = { BindTarget = SelectionExState } };
        }

        var slides = selection.Where(bp => bp.Item is Slide).Select(bp => (Slide)bp.Item).OrderBy(s => s.StartTime).ToList();

        if (slides.Count > 0)
        {
            yield return new TernaryStateToggleMenuItem("Omit initial tap") { State = { BindTarget = SelectionSlideNoTapState } };
            yield return new TernaryStateToggleMenuItem("Slide Break") { State = { BindTarget = selectionSlideBodyBreakState } };
            yield return new TernaryStateToggleMenuItem("Slide Ex") { State = { BindTarget = selectionSlideBodyExState } };
        }

        if (canMergeSlides(slides))
            yield return new OsuMenuItem("Merge slides", MenuItemType.Destructive, () => mergeSlides(slides));

        if (canUnmerge(slides))
            yield return new OsuMenuItem("Unmerge slides", MenuItemType.Destructive, () => unmergeSlides(slides));

        foreach (var item in base.GetContextMenuItemsForSelection(selection))
            yield return item;
    }

    private void moveTouchNotes(Vector2 dragDelta)
    {
        const float boundary_radius = 270;

        float dragDistance(Vector2 origin, Vector2 destination)
            => MathF.Min((destination - origin).Length, circleIntersectionDistance(origin, destination - origin));

        float circleIntersectionDistance(Vector2 centre, Vector2 direction)
        {
            direction.Normalize();
            float b = (direction.X * centre.X) + (direction.Y * centre.Y);
            float c = centre.LengthSquared - (boundary_radius * boundary_radius);
            return MathF.Sqrt((b * b) - c) - b;
        }

        var touches = SelectedBlueprints.Select(bp => (IHasPosition)bp.Item).ToList();
        Vector2 centre = touches.Aggregate(Vector2.Zero, (a, b) => a + b.Position) / touches.Count;
        float cappedDragDelta = touches.Min(t => dragDistance(t.Position - centre, t.Position + dragDelta));

        if (!(cappedDragDelta >= 0)) return; // No movement or invalid movement occurred

        var movementAmount = -centre + (cappedDragDelta * (dragDelta + centre).Normalized());

        foreach (var touch in touches)
        {
            switch (touch)
            {
                case Touch t:
                    t.Position += movementAmount;
                    break;

                case TouchHold th:
                    th.Position += movementAmount;
                    break;
            }
        }
    }

    private bool canMergeSlides(List<Slide> slides) => slides.Count > 1 && slides.GroupBy(s => s.Lane).Count() == 1;
    private bool canUnmerge(List<Slide> hitObjects) => hitObjects.Any(s => s.SlideInfoList.Count > 1);

    private void mergeSlides(List<Slide> slides)
    {
        var controlPointInfo = EditorBeatmap.ControlPointInfo;
        var firstHitObject = slides[0];
        var mergedSlide = new Slide
        {
            StartTime = firstHitObject.StartTime,
            SlideInfoList = firstHitObject.SlideInfoList,
            Samples = firstHitObject.Samples,
            Lane = firstHitObject.Lane,
            Break = firstHitObject.Break
        };

        double beatLength = controlPointInfo.TimingPointAt(mergedSlide.StartTime).BeatLength;

        for (int i = 1; i < slides.Count; ++i)
        {
            var slide = slides[i];
            double bodyBeatLength = controlPointInfo.TimingPointAt(slide.StartTime).BeatLength;

            foreach (var slideBody in slide.SlideInfoList)
            {
                double bodyOffset = (bodyBeatLength * slideBody.ShootDelay);
                double startTimeDelta = slide.StartTime - firstHitObject.StartTime;
                double newBodyOffset = startTimeDelta + bodyOffset;
                slideBody.ShootDelay = (float)(newBodyOffset / beatLength);
                slideBody.Duration += startTimeDelta;

                int similarBodyIndex = findSimilarBody(slideBody);

                if (similarBodyIndex != -1)
                {
                    mergedSlide.SlideInfoList[similarBodyIndex].Duration = slideBody.Duration;
                    continue;
                }

                mergedSlide.SlideInfoList.Add(slideBody);
            }
        }

        EditorBeatmap.BeginChange();

        foreach (var slide in slides)
            EditorBeatmap.Remove(slide);

        EditorBeatmap.Add(mergedSlide);
        SelectedItems.Add(mergedSlide);

        EditorBeatmap.EndChange();
        return;

        int findSimilarBody(SlideBodyInfo other)
        {
            for (int i = 0; i < mergedSlide!.SlideInfoList.Count; ++i)
            {
                if (mergedSlide.SlideInfoList[i].ShapeEquals(other))
                    return i;
            }

            return -1;
        }
    }

    private void unmergeSlides(List<Slide> slides)
    {
        EditorBeatmap.BeginChange();

        foreach (var slide in slides)
        {
            if (slide.SlideInfoList.Count <= 1)
                continue;

            for (int i = 0; i < slide.SlideInfoList.Count; ++i)
            {
                var slideInfo = slide.SlideInfoList[i];

                var newSlide = new Slide
                {
                    StartTime = slide.StartTime,
                    SlideInfoList = [slideInfo],
                    Samples = slide.Samples,
                    Lane = slide.Lane,
                    Break = slide.Break
                };

                EditorBeatmap.Add(newSlide);
            }

            EditorBeatmap.Remove(slide);
        }

        EditorBeatmap.EndChange();
    }
}
