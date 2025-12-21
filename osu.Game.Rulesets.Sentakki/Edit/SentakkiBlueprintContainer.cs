using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touches;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit;

[Cached]
public partial class SentakkiBlueprintContainer : ComposeBlueprintContainer
{
    public new SentakkiHitObjectComposer Composer => (SentakkiHitObjectComposer)base.Composer;

    [Cached]
    private DrawablePool<SlideChevron> chevrons { get; set; }

    public SentakkiBlueprintContainer(SentakkiHitObjectComposer composer)
        : base(composer)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        AddInternal(chevrons = new DrawablePool<SlideChevron>(100));
    }

    public override HitObjectSelectionBlueprint? CreateHitObjectBlueprintFor(HitObject hitObject)
    {
        switch (hitObject)
        {
            case Tap tap:
                return new TapSelectionBlueprint(tap);

            case Hold hold:
                return new HoldSelectionBlueprint(hold);

            case Slide slide:
                return new SlideSelectionBlueprint(slide);

            case Touch touch:
                return new TouchSelectionBlueprint(touch);

            case TouchHold touchHold:
                return new TouchHoldSelectionBlueprint(touchHold);

            default:
                return base.CreateHitObjectBlueprintFor(hitObject);
        }
    }

    protected override SelectionHandler<HitObject> CreateSelectionHandler() => new SentakkiSelectionHandler();

    private Vector2 currentMousePosition => InputManager.CurrentState.Mouse.Position;

    // Since the movement is going to be a rotation, it makes sense that we prioritise the closest hitobject.
    protected override IEnumerable<SelectionBlueprint<HitObject>> SortForMovement(IReadOnlyList<SelectionBlueprint<HitObject>> blueprints)
        => blueprints.OrderBy(b => Vector2.DistanceSquared(b.ScreenSpaceSelectionPoint, currentMousePosition));

    protected override bool TryMoveBlueprints(DragEvent e, IList<(SelectionBlueprint<HitObject> blueprint, Vector2[] originalSnapPositions)> blueprints)
    {
        Vector2 distanceTravelled = e.ScreenSpaceMousePosition - e.ScreenSpaceMouseDownPosition;

        // The final movement position, relative to movementBlueprintOriginalPosition.
        Vector2 movePosition = blueprints.First().originalSnapPositions.First() + distanceTravelled;

        if (blueprints.All(h => h.blueprint.Item is IHasPosition))
        {
            var snappedPosition = Composer.TouchPositionSnapGrid.GetSnappedPosition(ToLocalSpace(movePosition) - OriginPosition);

            if (snappedPosition is not null)
            {
                snappedPosition = ToScreenSpace(snappedPosition.Value + OriginPosition);
                Vector2 snapOffset = snappedPosition.Value - movePosition;

                foreach (var bp in blueprints)
                {
                    var snappedPosition2 = ToLocalSpace(bp.originalSnapPositions.First() + (distanceTravelled + snapOffset)) - OriginPosition;

                    if (Math.Round(snappedPosition2.Length) > 270)
                        return false;
                }

                movePosition = snappedPosition.Value;
            }
        }

        var movementEvent = new MoveSelectionEvent<HitObject>(blueprints.First().blueprint, movePosition - blueprints.First().blueprint.ScreenSpaceSelectionPoint);
        SelectionHandler.HandleMovement(movementEvent);
        return true;
    }
}
