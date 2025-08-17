using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI.Components;

public partial class HitExplosion : PoolableDrawable
{
    public override bool RemoveWhenNotAlive => true;

    private const float default_explosion_size = 75;
    private const float touch_hold_explosion_size = 100;

    public HitExplosion()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        Size = new Vector2(default_explosion_size);
        Colour = Color4.White;
        BorderColour = Color4.White;
        Alpha = 0;
        Masking = true;
        CornerExponent = 2;
        InternalChild = new Box
        {
            Alpha = 0,
            RelativeSizeAxes = Axes.Both,
            AlwaysPresent = true,
        };
    }

    private bool circular = true;

    protected override void Update()
    {
        base.Update();

        if (circular) // We mimic whatever CircularContainer does
            CornerRadius = Math.Min(DrawSize.X, DrawSize.Y) * 0.5f;
        else
            CornerRadius = 20;
    }

    public HitExplosion Apply(DrawableSentakkiHitObject drawableSentakkiHitObject)
    {
        Colour = drawableSentakkiHitObject.AccentColour.Value;

        switch (drawableSentakkiHitObject.HitObject)
        {
            case SentakkiLanedHitObject lanedObject:
                Position = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, lanedObject.Lane);
                Size = new Vector2(default_explosion_size);
                circular = true;
                break;

            case Touch touchObject:
                Position = touchObject.Position;
                Size = new Vector2(default_explosion_size);
                circular = false;
                Rotation = 0;
                break;

            case TouchHold touchHoldObject:
                Position = touchHoldObject.Position;
                Size = new Vector2(touch_hold_explosion_size);
                circular = false;
                Rotation = 45;
                break;
        }

        return this;
    }

    protected override void PrepareForUse()
    {
        base.PrepareForUse();

        // When used as a standalone piece in TOUCH and TOUCHHOLDs, we want to avoid the animation playing out immediately
        if (!IsInPool)
            return;

        Explode().Expire(true);
    }

    public TransformSequence<HitExplosion> Explode()
    {
        const double explode_duration = 80;
        var sequence = this.FadeTo(0.8f)
                           .TransformTo(nameof(BorderThickness), Size.X * 0.5f)
                           .ScaleTo(1)
                           .Then()
                           .TransformTo(nameof(BorderThickness), 0f, duration: explode_duration)
                           .ScaleTo(2f, explode_duration)
                           .FadeOut(explode_duration);

        return sequence;
    }
}
