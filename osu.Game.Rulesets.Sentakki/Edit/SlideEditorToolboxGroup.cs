using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Edit.Toolbox;
using osu.Framework.Bindables;
using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osuTK.Input;
using System;


namespace osu.Game.Rulesets.Sentakki.Edit;

[Cached]
public partial class SlideEditorToolboxGroup : EditorToolboxGroup
{
    // Slide info
    private Bindable<SlidePaths.PathShapes> shapeBindable = new Bindable<SlidePaths.PathShapes>();
    public readonly Bindable<int> LaneOffset = new Bindable<int>(4);
    private Bindable<bool> mirrored = new Bindable<bool>();

    private Bindable<float> shootDelay = new Bindable<float>();

    public readonly Bindable<SlideBodyPart> CurrentPartBindable = new Bindable<SlideBodyPart>(new SlideBodyPart(SlidePaths.PathShapes.Straight, 4, false));

    public SlideBodyPart CurrentPart => CurrentPartBindable.Value;

    public SlideEditorToolboxGroup() : base("slide")
    {
        Children = new Drawable[]{
            new ExpandableMenu<SlidePaths.PathShapes>("Shape"){
                Current = shapeBindable
            },
            new ExpandableCounter<int>("Lane offset"){
                Current = LaneOffset
            },
            new ExpandableCounter<float>("Shoot delay"){
                Current = shootDelay
            },
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        shapeBindable.BindValueChanged(e => onShapeChanged());
        LaneOffset.BindValueChanged(e => onShapeChanged());
        mirrored.BindValueChanged(e => onShapeChanged());
    }

    [Resolved]
    private IBeatSnapProvider beatSnapProvider { get; set; } = null!;

    public void ChangeTarget(SlidePlacementBlueprint? slideBlueprint)
    {
        if (slideBlueprint is null)
            Hide();

        Show();
    }

    private void onShapeChanged()
    {
        int oldOffset = CurrentPart.EndOffset;
        int newOffset = LaneOffset.Value;

        SlideBodyPart newPart;

        bool tryNegativeDeltaFirst = (oldOffset - newOffset) < 0;
        int shiftAmount = 0;

        do
        {
            bool negative = ((shiftAmount % 2) == 0) ^ tryNegativeDeltaFirst;
            newPart = new SlideBodyPart(shapeBindable.Value, newOffset + shiftAmount * (negative ? -1 : 1), mirrored.Value);

            if (CurrentPart.Equals(newPart))
                break;

            shiftAmount++;
        }
        while (!SlidePaths.CheckSlideValidity(newPart));

        CurrentPartBindable.Value = newPart;
        LaneOffset.Value = newPart.EndOffset;
    }

    protected override bool OnKeyDown(KeyDownEvent e)
    {
        switch (e.Key)
        {
            case Key.Plus:
                shootDelay.Value += 1f / beatSnapProvider.BeatDivisor;
                break;

            case Key.Minus:
                shootDelay.Value = Math.Max(0, shootDelay.Value - 1f / beatSnapProvider.BeatDivisor);
                break;

            case Key.Number0:
                shootDelay.Value = 1;
                break;

            case Key.BackSlash:
                mirrored.Value = !mirrored.Value;
                return true;

            case Key.BracketRight:
                shapeBindable.Value = (SlidePaths.PathShapes)((int)(shapeBindable.Value + 1) % 8);
                return true;

            case Key.BracketLeft:
                shapeBindable.Value = (SlidePaths.PathShapes)((int)(shapeBindable.Value + 7) % 8);
                return true;
        }

        return base.OnKeyDown(e);
    }

    private partial class LaneOffsetCounter : ExpandableCounter<int>
    {
        public LaneOffsetCounter() : base("Lane offset")
        {
        }

        protected override void OnLeftButtonPressed() => Current.Value = (Current.Value - 1) % 8;
        protected override void OnRightButtonPressed() => Current.Value = (Current.Value + 1) % 8;
    }

    private partial class ShootDelayCounter : ExpandableCounter<float>
    {
        [Resolved]
        private IBeatSnapProvider beatSnapProvider { get; set; } = null!;

        public ShootDelayCounter() : base("Lane offset")
        {
        }

        protected override void OnLeftButtonPressed() => Current.Value += 1f / beatSnapProvider.BeatDivisor;
        protected override void OnRightButtonPressed() => Current.Value = Math.Max(0, Current.Value - 1f / beatSnapProvider.BeatDivisor);
    }

}
