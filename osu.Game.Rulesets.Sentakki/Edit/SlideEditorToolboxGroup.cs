using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;
using osu.Game.Rulesets.Sentakki.Edit.Toolbox;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit;

[Cached]
public partial class SlideEditorToolboxGroup : EditorToolboxGroup
{
    // Slide info
    private readonly Bindable<SlidePaths.PathShapes> shapeBindable = new Bindable<SlidePaths.PathShapes>();
    public readonly Bindable<int> LaneOffset = new Bindable<int>(4);
    private readonly Bindable<bool> mirrored = new Bindable<bool>();

    public Bindable<float> ShootDelayBindable = new Bindable<float>(1);

    public readonly Bindable<SlideBodyPart> CurrentPartBindable = new Bindable<SlideBodyPart>(new SlideBodyPart(SlidePaths.PathShapes.Straight, 4, false));

    public SlideBodyPart CurrentPart => CurrentPartBindable.Value;
    public float CurrentShootDelay => ShootDelayBindable.Value;

    public SlideEditorToolboxGroup()
        : base("slide")
    {
        Children = new Drawable[]
        {
            new ExpandableMenu<SlidePaths.PathShapes>("Shape")
            {
                Current = shapeBindable
            },
            new LaneOffsetCounter
            {
                Current = LaneOffset
            },
            new ShootDelayCounter()
            {
                Current = ShootDelayBindable
            },
            new ExpandableCheckbox("Mirrored")
            {
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
            var newPart = new SlideBodyPart(shapeBindable.Value, (newLane + (i * rotationFactor)).NormalizeLane(), mirrored.Value);

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

    public bool HandleKeyDown(KeyDownEvent e)
    {
        switch (e.Key)
        {
            // Dedicated keybinds for shapes
            case Key.Number1:
            case Key.Number2:
            case Key.Number3:
            case Key.Number4:
            case Key.Number5:
            case Key.Number6:
            case Key.Number7:
                if (!e.AltPressed)
                    break;

                int index = e.Key - Key.Number1;

                SlidePaths.PathShapes targetShape = (SlidePaths.PathShapes)index;

                if (shapeBindable.Value == targetShape)
                    mirrored.Value = !mirrored.Value;

                shapeBindable.Value = targetShape;

                return true;

            case Key.D:
                ShootDelayBindable.Value += 1f / beatSnapProvider.BeatDivisor;
                return true;

            case Key.A:
                ShootDelayBindable.Value = Math.Max(0, ShootDelayBindable.Value - (1f / beatSnapProvider.BeatDivisor));
                return true;

            case Key.S:
                ShootDelayBindable.Value = 1;
                return true;

            case Key.Tab:
                if (e.ControlPressed)
                    mirrored.Value = !mirrored.Value;
                else if (e.ShiftPressed)
                    shapeBindable.Value = (SlidePaths.PathShapes)((int)(shapeBindable.Value + 6) % 7);
                else
                    shapeBindable.Value = (SlidePaths.PathShapes)((int)(shapeBindable.Value + 1) % 7);

                return true;
        }

        return false;
    }

    private partial class LaneOffsetCounter : ExpandableCounter<int>
    {
        public LaneOffsetCounter()
            : base("Lane offset")
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

        public ShootDelayCounter()
            : base("Shoot delay", @"{0:0.##} beats")
        {
        }

        protected override void OnLeftButtonPressed() => Current.Value = Math.Max(0, Current.Value - (1f / beatSnapProvider.BeatDivisor));

        protected override void OnRightButtonPressed() => Current.Value += 1f / beatSnapProvider.BeatDivisor;
    }
}
