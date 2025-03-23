using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touches;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public partial class SentakkiBlueprintContainer : ComposeBlueprintContainer
    {
        public new SentakkiHitObjectComposer Composer => (SentakkiHitObjectComposer)base.Composer;
        public SentakkiBlueprintContainer(HitObjectComposer composer)
            : base(composer)
        {
            AddInternal(chevronPool = new DrawablePool<SlideChevron>(100));
        }

        [Cached]
        private DrawablePool<SlideChevron> chevronPool;

        protected override SelectionHandler<HitObject> CreateSelectionHandler() => new SentakkiSelectionHandler();

        protected override bool TryMoveBlueprints(DragEvent e, IList<(SelectionBlueprint<HitObject> blueprint, Vector2[] originalSnapPositions)> blueprints)
        {
            var sentakkiPlayfield = Composer.Playfield;
            Vector2 distanceTravelled = e.ScreenSpaceMousePosition - e.ScreenSpaceMouseDownPosition;

            // The final movement position, relative to movementBlueprintOriginalPosition.
            Vector2 movePosition = blueprints.First().originalSnapPositions.First() + distanceTravelled;
            SnapResult senSnapResult = Composer.FindSnappedPositionAndTime(movePosition);

            var referenceBlueprint = blueprints.First().blueprint;
            bool moved = SelectionHandler.HandleMovement(new MoveSelectionEvent<HitObject>(referenceBlueprint, senSnapResult.ScreenSpacePosition - referenceBlueprint.ScreenSpaceSelectionPoint));
            if (moved)
                ApplySnapResultTime(senSnapResult, referenceBlueprint.Item.StartTime);

            if (blueprints.All(b => b.blueprint.Item is SentakkiLanedHitObject))
            {
                int offset = ((SentakkiLanedSnapResult)senSnapResult).Lane - ((SentakkiLanedHitObject)blueprints.First().blueprint.Item).Lane;
                if (offset != 0)
                {
                    Beatmap.PerformOnSelection(ho =>
                    {
                        var lho = (SentakkiLanedHitObject)ho;

                        sentakkiPlayfield.Remove(lho);
                        lho.Lane = (lho.Lane + offset).NormalizePath();
                        sentakkiPlayfield.Add(lho);
                        Beatmap.Update(ho);
                    });
                }
            }

            return true;
        }

        public override HitObjectSelectionBlueprint CreateHitObjectBlueprintFor(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Tap t:
                    return new TapSelectionBlueprint(t);

                case Hold h:
                    return new HoldSelectionBlueprint(h);

                case Touch t:
                    return new TouchSelectionBlueprint(t);

                case TouchHold th:
                    return new TouchHoldSelectionBlueprint(th);

                case Slide s:
                    return new SlideSelectionBlueprint(s);
            }

            return base.CreateHitObjectBlueprintFor(hitObject);
        }

        private Vector2 currentMousePosition => InputManager.CurrentState.Mouse.Position;

        protected override IEnumerable<SelectionBlueprint<HitObject>> SortForMovement(IReadOnlyList<SelectionBlueprint<HitObject>> blueprints)
            => blueprints.OrderBy(b => Vector2.DistanceSquared(b.ScreenSpaceSelectionPoint, currentMousePosition));
    }
}
