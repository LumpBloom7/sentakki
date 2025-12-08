using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps;

public partial class TapPlacementBlueprint : SentakkiPlacementBlueprint<Tap>
{
    private readonly TapPiece highlight;

    public TapPlacementBlueprint()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        InternalChild = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Child = highlight = new TapPiece
            {
                Alpha = 0.5f,
                Colour = Color4.YellowGreen
            }
        };
    }

    private bool initialStateApplied;

    protected override void Update()
    {
        base.Update();
        float newRotation = HitObject.Lane.GetRotationForLane();

        highlight.Y = -SentakkiPlayfield.INTERSECTDISTANCE;

        if (!initialStateApplied)
        {
            InternalChild.Rotation = newRotation;
            initialStateApplied = true;
        }

        float angleDelta = MathExtensions.AngleDelta(InternalChild.Rotation, newRotation);
        InternalChild.Rotation += 25 * angleDelta * (float)(Time.Elapsed / 1000);
    }

    public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double time)
    {
        var localPosition = ToLocalSpace(screenSpacePosition);

        if (Vector2.Distance(OriginPosition, localPosition) < 100)
            return base.UpdateTimeAndPosition(screenSpacePosition, time);

        float angle = OriginPosition.AngleTo(localPosition);

        HitObject.Lane = (int)Math.Round((angle - 22.5f) / 45);

        return base.UpdateTimeAndPosition(screenSpacePosition, time);
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (e.Button != MouseButton.Left) return false;

        EndPlacement(true);
        return true;
    }
}
