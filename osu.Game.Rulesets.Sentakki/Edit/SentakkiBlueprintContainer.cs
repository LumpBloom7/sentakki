using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touches;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit;

[Cached]
public partial class SentakkiBlueprintContainer : ComposeBlueprintContainer
{
    public new SentakkiHitObjectComposer Composer => (SentakkiHitObjectComposer)base.Composer;

    private SentakkiMovementHandler movementHandler;

    public new SentakkiSelectionHandler SelectionHandler => (SentakkiSelectionHandler)base.SelectionHandler;

    [Cached]
    private DrawablePool<SlideChevron> chevrons { get; set; }

    public SentakkiBlueprintContainer(SentakkiHitObjectComposer composer)
        : base(composer)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        AddInternal(chevrons = new DrawablePool<SlideChevron>(100));
        AddInternal(movementHandler = new SentakkiMovementHandler());
    }

    [BackgroundDependencyLoader]
    private void load(EditorBeatmap beatmap)
    {
        Schedule(() => ungroupAllSlides(beatmap));
    }

    private static void ungroupAllSlides(EditorBeatmap editorBeatmap)
    {
        // Find all slides, and decompose them into individual parts
        var slides = editorBeatmap.HitObjects.OfType<Slide>().Where(s => s.SlideInfoList.Count > 1).ToList();

        editorBeatmap.RemoveRange(slides);

        List<Slide> newSlides = [];

        foreach (var slide in slides)
        {
            foreach (var slideBodyInfo in slide.SlideInfoList)
            {
                newSlides.Add(new Slide
                {
                    Lane = slide.Lane,
                    StartTime = slide.StartTime,
                    SlideInfoList = [slideBodyInfo],
                    Samples = slide.Samples,
                    TapType = slide.TapType,
                    Break = slide.Break,
                    Ex = slide.Ex
                });
            }
        }

        editorBeatmap.AddRange(newSlides);
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

    // In the playfield, we actually proxy the slides to be under every other note type, and touches to be above.
    // The default selection order doesn't take that into account when determining the click target, which results in targetting the bottom note instead of the top.
    // We apply the proxy precedence rules Touch -> TouchHold -> Tap/Holds -> Slides, before applying time based precedence
    protected override IEnumerable<SelectionBlueprint<HitObject>> ApplySelectionOrder(IEnumerable<SelectionBlueprint<HitObject>> blueprints)
        => blueprints.Reverse()
        .OrderBy(b => b.Item is not Touch)
        .ThenBy(b => b.Item is not TouchHold)
        .ThenBy(b => b.Item is Slide)
        .ThenBy(b => Math.Min(Math.Abs(EditorClock.CurrentTime - b.Item.GetEndTime()), Math.Abs(EditorClock.CurrentTime - b.Item.StartTime)));

    // Since the movement is going to be a rotation, it makes sense that we prioritise the closest hitobject.
    protected override IEnumerable<SelectionBlueprint<HitObject>> SortForMovement(IReadOnlyList<SelectionBlueprint<HitObject>> blueprints)
        => blueprints.OrderBy(b => Vector2.DistanceSquared(b.ScreenSpaceSelectionPoint, currentMousePosition));

    protected override bool TryMoveBlueprints(DragEvent e, IList<(SelectionBlueprint<HitObject> blueprint, Vector2[] originalSnapPositions)> blueprints)
        => movementHandler.TryMoveBlueprints(e, blueprints);

    // <HACK ZONE>
    // Some blueprints can be interactive parts, such as draggable elements.
    // We don't handle OnMouseDown in order to preserve functionality provided by the BlueprintContainer.
    // However, because of that, BlueprintContainer will always run OnMouseUp regardless of whether a drag is handled on a target blueprint.
    // We provide a mechanism for interactive blueprints to inform the BlueprintContainer that they "handled" OnMouseUp,
    //   and that the BlueprintContainer doesn't need to do anything
    private bool suppressMouseUp;
    public void SuppressMouseUp()
    {
        suppressMouseUp = true;
    }

    protected override void OnMouseUp(MouseUpEvent e)
    {
        if (suppressMouseUp)
        {
            suppressMouseUp = false;
            return;
        }

        base.OnMouseUp(e);
    }
}
