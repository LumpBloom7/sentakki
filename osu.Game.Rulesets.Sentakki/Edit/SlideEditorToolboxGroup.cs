using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;
using osu.Game.Rulesets.Sentakki.Edit.Toolbox;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit;

[Cached]
public partial class SlideEditorToolboxGroup : EditorToolboxGroup
{
    // Slide info
    private Bindable<SlidePaths.PathShapes> shapeBindable = new Bindable<SlidePaths.PathShapes>(); // This is locked
    public readonly Bindable<int> LaneOffset = new Bindable<int>(4);
    private Bindable<bool> mirrored = new Bindable<bool>(); // This is locked

    public Bindable<float> ShootDelayBindable = new Bindable<float>();

    public readonly Bindable<SlideBodyPart> CurrentPartBindable = new Bindable<SlideBodyPart>(new SlideBodyPart(SlidePaths.PathShapes.Straight, 4, false));

    public SlideBodyPart CurrentPart => CurrentPartBindable.Value;
    public float CurrentShootDelay => ShootDelayBindable.Value;

    public SlideEditorToolboxGroup() : base("slide")
    {
        Children = new Drawable[]{
            new ExpandableMenu<SlidePaths.PathShapes>("Shape"){
                Current = shapeBindable
            },
            new LaneOffsetCounter{
                Current = LaneOffset
            },
            new ShootDelayCounter(){
                Current = ShootDelayBindable
            },
            new ExpandableCheckbox("Mirrored"){
                Current = mirrored
            }
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        shapeBindable.BindValueChanged(e => RequestLaneChange(LaneOffset.Value));
        mirrored.BindValueChanged(e => RequestLaneChange(LaneOffset.Value));
    }

    [Resolved]
    private IBeatSnapProvider beatSnapProvider { get; set; } = null!;

    public void ChangeTarget(SlidePlacementBlueprint? slideBlueprint)
    {
        if (slideBlueprint is null)
            Hide();

        Show();
    }

    public void RequestLaneChange(int newLane, bool findClosestMatch = false)
    {
        int oldOffset = LaneOffset.Value;

        int rotationFactor = newLane - oldOffset >= 0 ? 1 : -1;

        for (int i = 0; i < 8; ++i)
        {
            var newPart = new SlideBodyPart(shapeBindable.Value, (newLane + (i * rotationFactor)).NormalizePath(), mirrored.Value);

            if (SlidePaths.CheckSlideValidity(newPart))
            {
                CurrentPartBindable.Value = newPart;
                LaneOffset.Value = newPart.EndOffset;
                return;
            }
            if (findClosestMatch)
                rotationFactor *= -1;
        }
    }

    protected override bool OnKeyDown(KeyDownEvent e)
    {
        switch (e.Key)
        {
            case Key.Plus when e.AltPressed:
                ShootDelayBindable.Value += 1f / beatSnapProvider.BeatDivisor;
                return true;

            case Key.Minus when e.AltPressed:
                ShootDelayBindable.Value = Math.Max(0, ShootDelayBindable.Value - (1f / beatSnapProvider.BeatDivisor));
                return true;

            case Key.Number0 when e.AltPressed:
                ShootDelayBindable.Value = 1;
                return true;

            case Key.BackSlash:
                mirrored.Value = !mirrored.Value;
                return true;

            case Key.BracketRight:
                shapeBindable.Value = (SlidePaths.PathShapes)((int)(shapeBindable.Value + 1) % 8);
                return true;

            case Key.BracketLeft:
                shapeBindable.Value = (SlidePaths.PathShapes)(((int)(shapeBindable.Value + 7)) % 8);
                return true;
        }

        return base.OnKeyDown(e);
    }

    private partial class LaneOffsetCounter : ExpandableCounter<int>
    {
        public LaneOffsetCounter() : base("Lane offset")
        {
        }

        [Resolved]
        private SlideEditorToolboxGroup slideEditorToolboxGroup { get; set; } = null!;

        protected override void OnLeftButtonPressed() => slideEditorToolboxGroup.RequestLaneChange(Current.Value - 1);
        protected override void OnRightButtonPressed() => slideEditorToolboxGroup.RequestLaneChange(Current.Value + 1);
    }

    private partial class ShootDelayCounter : ExpandableCounter<float>
    {
        [Resolved]
        private IBeatSnapProvider beatSnapProvider { get; set; } = null!;

        public ShootDelayCounter() : base("Shoot delay", @"{0:0.##} beats")
        {
        }

        protected override void OnLeftButtonPressed() => Current.Value = Math.Max(0, Current.Value - (1f / beatSnapProvider.BeatDivisor));

        protected override void OnRightButtonPressed() => Current.Value += 1f / beatSnapProvider.BeatDivisor;
    }
}
