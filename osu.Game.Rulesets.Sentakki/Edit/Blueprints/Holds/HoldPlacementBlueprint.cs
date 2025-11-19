using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds;

public partial class HoldPlacementBlueprint : SentakkiPlacementBlueprint<Hold>
{
    public HoldPlacementBlueprint()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        InternalChild = new HoldBody()
        {
            Colour = Color4.YellowGreen,
            Alpha = 0.5f,
        };
    }

    private readonly Bindable<float> animationSpeed = new Bindable<float>(5);

    [BackgroundDependencyLoader]
    private void load(SentakkiRulesetConfigManager configs)
    {
        configs.BindWith(SentakkiRulesetSettings.AnimationSpeed, animationSpeed);
    }

    protected override void Update()
    {
        Rotation = HitObject.Lane.GetRotationForLane();

        const float max_height = SentakkiPlayfield.INTERSECTDISTANCE - SentakkiPlayfield.NOTESTARTDISTANCE;
        double animationDuration = DrawableSentakkiRuleset.ComputeLaneNoteEntryTime(animationSpeed.Value) / 2;

        // TODO: Needs better variable names
        double headY = Math.Min(HitObject.StartTime - EditorClock.CurrentTime, animationDuration);
        double tailY = Math.Min(HitObject.EndTime - EditorClock.CurrentTime, animationDuration);
        double height = tailY - headY;

        InternalChild.Y = (float)(-SentakkiPlayfield.INTERSECTDISTANCE + headY / animationDuration * max_height);
        InternalChild.Height = (float)(height / animationDuration * max_height);
    }

    private double commitStartTime;

    public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double time)
    {
        switch (PlacementActive)
        {
            case PlacementState.Waiting:
                float angle = ToScreenSpace(OriginPosition).AngleTo(screenSpacePosition);

                HitObject.Lane = (int)Math.Round((angle - 22.5f) / 45);
                break;

            case PlacementState.Active:
                // If the mapper maps the hold in reverse direction, we swap the start and end times to ensure correctness.
                HitObject.StartTime = Math.Min(commitStartTime, time);
                HitObject.EndTime = Math.Max(commitStartTime, time);
                break;
        }

        return base.UpdateTimeAndPosition(screenSpacePosition, time);
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (e.Button is not MouseButton.Left)
            return base.OnMouseDown(e);

        switch (PlacementActive)
        {
            case PlacementState.Waiting:
                BeginPlacement(true);
                commitStartTime = HitObject.StartTime;
                return true;

            case PlacementState.Active:
                EndPlacement(true);
                return true;
        }

        return base.OnMouseDown(e);
    }
}
