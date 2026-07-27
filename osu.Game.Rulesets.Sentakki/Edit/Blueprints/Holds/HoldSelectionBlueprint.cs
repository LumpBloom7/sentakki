using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.Skinning;
using osu.Game.Rulesets.Sentakki.Skinning.Default;
using osu.Game.Screens.Edit;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds;

public partial class HoldSelectionBlueprint : SentakkiSelectionBlueprint<Hold, DrawableHold>
{
    public override Quad SelectionQuad => highlight.ScreenSpaceDrawQuad;
    public override Vector2 ScreenSpaceSelectionPoint => startDot.ScreenSpaceDrawQuad.Centre;

    private Container highlightContainer;
    private readonly ProxyableSkinnableDrawable highlight;

    private Drawable startDot;

    public HoldSelectionBlueprint(Hold item)
        : base(item)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        InternalChild = highlightContainer = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.TopCentre,

            Children = [
                highlight = new ProxyableSkinnableDrawable(new SentakkiSkinComponentLookup(SentakkiSkinComponents.Hold), _ => new HoldBody())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.YellowGreen,
                },
                startDot = new DraggableDotPiece(){
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.Centre,
                    DragAction = adjustStartTime
                },
                new DraggableDotPiece(){
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre,
                    DragAction = adjustEndTime
                }
            ]
        };
    }

    [Resolved]
    private LaneNoteSnapGrid snapGrid { get; set; } = null!;

    [Resolved]
    private EditorBeatmap editorBeatmap { get; set; } = null!;

    private void adjustStartTime(Vector2 screenSpacePosition)
    {
        var localMousePosition = ToLocalSpace(screenSpacePosition) - OriginPosition;

        double currentStartTime = snapGrid.GetSnappedTimeAndPosition(Item.StartTime, localMousePosition).snappedTime;
        double currentEndTime = Item.EndTime;

        Item.StartTime = Math.Min(currentStartTime, currentEndTime);
        Item.EndTime = Math.Max(currentEndTime, currentStartTime);

        editorBeatmap.Update(Item);
    }

    private void adjustEndTime(Vector2 screenSpacePosition)
    {
        var localMousePosition = ToLocalSpace(screenSpacePosition) - OriginPosition;

        double currentStartTime = Item.StartTime;
        double currentEndTime = snapGrid.GetSnappedTimeAndPosition(Item.EndTime, localMousePosition).snappedTime;

        Item.StartTime = Math.Min(currentStartTime, currentEndTime);
        Item.EndTime = Math.Max(currentEndTime, currentStartTime);

        editorBeatmap.Update(Item);
    }

    protected override void Update()
    {
        Rotation = HitObject.Lane.GetRotationForLane();
        highlightContainer.Y = DrawableObject.Y;
        highlightContainer.Scale = DrawableObject.Scale;
        highlightContainer.Height = DrawableObject.Height;
    }

    private partial class DraggableDotPiece : DotPiece
    {
        [Resolved]
        private LaneNoteSnapGrid snapGrid { get; set; } = null!;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            if (snapGrid.State.Value is Visibility.Hidden)
                return false;

            if (IsDragged)
                return true;

            return base.ReceivePositionalInputAt(screenSpacePos);
        }

        [Resolved]
        private SentakkiBlueprintContainer blueprintContainer { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Alpha = 0;
            AlwaysPresent = true;
            Colour = colours.YellowDark;
        }

        public Action<Vector2> DragAction { get; init; } = null!;

        protected override void OnDrag(DragEvent e)
        {
            DragAction(e.ScreenSpaceMousePosition);
            base.OnDrag(e);
        }

        private bool dragOccured;

        protected override bool OnDragStart(DragStartEvent e) => dragOccured = DragAction is not null;

        protected override void OnMouseUp(MouseUpEvent e)
        {
            // HACK: Blueprint container will attempt to perform selection actions, jankily supress it.
            if (dragOccured)
                blueprintContainer.SuppressMouseUp();

            base.OnMouseUp(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            this.ScaleTo(1.3f, 50).FadeIn(50);

            // We don't "handle" the hover, allowing the blueprint container to accept click events
            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            this.ScaleTo(1f, 100).FadeOut(100);
        }
    }
}
