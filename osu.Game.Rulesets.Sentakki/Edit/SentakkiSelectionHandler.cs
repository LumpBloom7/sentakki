using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public partial class SentakkiSelectionHandler : EditorSelectionHandler
    {
        private readonly Bindable<TernaryState> selectionBreakState = new Bindable<TernaryState>();
        private readonly Bindable<TernaryState> selectionSlideBodyBreakState = new Bindable<TernaryState>();

        public SentakkiSelectionHandler()
        {
            selectionBreakState.ValueChanged += s =>
            {
                switch (s.NewValue)
                {
                    case TernaryState.False:
                    case TernaryState.True:
                        setBreakState(s.NewValue == TernaryState.True);
                        break;
                }
            };
            selectionSlideBodyBreakState.ValueChanged += s =>
            {
                switch (s.NewValue)
                {
                    case TernaryState.False:
                    case TernaryState.True:
                        setSlideBodyBreakState(s.NewValue == TernaryState.True);
                        break;
                }
            };
        }

        public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent)
        {
            // The lanes are arranged in a circular fashion
            // Blindly deriving new angle by adding the vector to each selection point will yield unintuitive results, such as all notes moving towards the drag direction
            //
            // Instead, we should derive an angle delta by comparing the current mouse position and the drag origin
            // The drag origin is the blueprint most closest to the mouse during the beginning of the drag, rather than the earliest note (see "SentakkiBlueprintContainer.SortForMovement")
            // This allows a more intuitive "steering wheel" like lane adjustments
            if (SelectedBlueprints.All(bp => bp.Item is SentakkiLanedHitObject))
            {
                var oldPosition = moveEvent.Blueprint.ScreenSpaceSelectionPoint;
                var newPosition = moveEvent.Blueprint.ScreenSpaceSelectionPoint + moveEvent.ScreenSpaceDelta;
                var playfieldCentre = ToScreenSpace(new Vector2(300));
                float angleDelta = playfieldCentre.GetDegreesFromPosition(newPosition) - playfieldCentre.GetDegreesFromPosition(oldPosition);

                foreach (var bp in SelectedBlueprints.ToList())
                {
                    var laned = (SentakkiLanedHitObject)bp.Item;
                    float currentAngle = laned.Lane.GetRotationForLane() + angleDelta;
                    laned.Lane = currentAngle.GetNoteLaneFromDegrees();
                }

                return true;
            }

            if (SelectedBlueprints.All(bp => bp.Item is Touch))
            {
                // Special movement handling to ensure that all touch notes are within 250 units from the playfield centre
                moveTouchNotes(this.ScreenSpaceDeltaToParentSpace(moveEvent.ScreenSpaceDelta));
                return true;
            }

            return false;
        }

        private void setBreakState(bool state)
        {
            var lhos = EditorBeatmap.SelectedHitObjects.OfType<SentakkiLanedHitObject>();

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

        protected override void UpdateTernaryStates()
        {
            base.UpdateTernaryStates();

            selectionBreakState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<SentakkiLanedHitObject>(), h => h.Break);
            selectionSlideBodyBreakState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<Slide>().SelectMany(h => h.SlideInfoList), s => s.Break);
        }

        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint<HitObject>> selection)
        {
            if (selection.Any(s => s.Item is SentakkiLanedHitObject))
                yield return new TernaryStateToggleMenuItem("Break") { State = { BindTarget = selectionBreakState } };

            var slides = selection.Where(bp => bp.Item is Slide).Select(bp => (Slide)bp.Item).OrderBy(s => s.StartTime).ToList();

            if (slides.Count > 0)
                yield return new TernaryStateToggleMenuItem("Slide Break") { State = { BindTarget = selectionSlideBodyBreakState } };

            if (canMergeSlides(slides))
                yield return new OsuMenuItem("Merge slides", MenuItemType.Destructive, () => mergeSlides(slides));

            if (canUnmerge(slides))
                yield return new OsuMenuItem("Unmerge slides", MenuItemType.Destructive, () => unmergeSlides(slides));

            foreach (var item in base.GetContextMenuItemsForSelection(selection))
                yield return item;
        }

        private void moveTouchNotes(Vector2 dragDelta)
        {
            const float boundary_radius = 250;

            float dragDistance(Vector2 origin, Vector2 destination)
                => MathF.Min((destination - origin).Length, circleIntersectionDistance(origin, destination - origin));

            float circleIntersectionDistance(Vector2 centre, Vector2 direction)
            {
                direction.Normalize();
                float b = (direction.X * centre.X) + (direction.Y * centre.Y);
                float c = centre.LengthSquared - (boundary_radius * boundary_radius);
                return MathF.Sqrt((b * b) - c) - b;
            }

            var touches = SelectedBlueprints.Select(bp => (Touch)bp.Item).ToList();
            Vector2 centre = touches.Aggregate(Vector2.Zero, (a, b) => a + b.Position) / touches.Count;
            float cappedDragDelta = touches.Min(t => dragDistance(t.Position - centre, t.Position + dragDelta));

            if (!(cappedDragDelta >= 0)) return; // No movement or invalid movement occurred

            foreach (var touch in touches)
                touch.Position = touch.Position - centre + (cappedDragDelta * (dragDelta + centre).Normalized());
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
                NodeSamples = firstHitObject.NodeSamples,
                Lane = firstHitObject.Lane,
                Break = firstHitObject.Break
            };

            double beatLength = controlPointInfo.TimingPointAt(mergedSlide.StartTime).BeatLength;

            int findSimilarBody(SlideBodyInfo other)
            {
                for (int i = 0; i < mergedSlide!.SlideInfoList.Count; ++i)
                    if (mergedSlide.SlideInfoList[i].ShapeEquals(other))
                        return i;

                return -1;
            }

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
                    var cpi = EditorBeatmap.ControlPointInfo;
                    double beatLengthOriginal = cpi.TimingPointAt(slide.StartTime).BeatLength;
                    double newSt = slide.StartTime + ((slideInfo.ShootDelay - 1) * beatLengthOriginal);

                    slideInfo.ShootDelay = 1;
                    slideInfo.Duration -= newSt - slide.Duration;

                    var newSlide = new Slide
                    {
                        StartTime = newSt,
                        SlideInfoList = new List<SlideBodyInfo> { slideInfo },
                        Samples = slide.Samples,
                        NodeSamples = slide.NodeSamples,
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
}
