using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.Skinning.Common;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy.TouchHold;

public partial class LegacyTouchHoldBody : CompositeDrawable, IHasCopyableVisualState
{
    public LegacyTouchHoldProgress ProgressPiece = null!;

    private Container mainPiece = null!;

    private Container trianglePieces = null!;

    public LegacyTouchHoldBody()
    {
        RelativeSizeAxes = Axes.Both;
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
    }

    [Resolved]
    private DrawableTouchHold? drawableTouchHold { get; set; } = null!;

    [Resolved]
    private Bindable<IReadOnlyList<Color4>>? paletteBindable { get; set; }

    [BackgroundDependencyLoader]
    private void load(ISkinSource skin)
    {
        InternalChildren = [
            ProgressPiece = new LegacyTouchHoldProgress(){
                RelativeSizeAxes = Axes.Both
            },
            mainPiece = new Container
            {
                Size = new Vector2(130),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = [
                    createTouchShadow(skin),
                    trianglePieces = createCentrePiece(skin),
                ]
            },
            new DotPiece(),
        ];

        if (drawableTouchHold is null)
            return;

        drawableTouchHold.ApplyCustomUpdateState += applyCustomUpdateState;
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        paletteBindable?.BindValueChanged(p =>
        {
            for (int i = 0; i < trianglePieces.Count; ++i)
                trianglePieces[i].Colour = p.NewValue[i];
        }, true);
    }

    public void CopyVisualStateTo(IHasCopyableVisualState other)
    {
        if (other is not LegacyTouchHoldBody otherTouchHoldBody)
            return;

        otherTouchHoldBody.ProgressPiece.ProgressBindable.Value = ProgressPiece.ProgressBindable.Value;
    }

    private Container createCentrePiece(ISkinSource skin)
    {
        return createTouchShape(skin);
    }

    private Container createTouchShape(ISkinSource skin)
    {
        var texture = skin.GetTexture("sentakki/touchhold");

        var container = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Rotation = 45,
        };

        Anchor[] anchors = [Anchor.TopCentre, Anchor.CentreRight, Anchor.BottomCentre, Anchor.CentreLeft];

        for (int i = 0; i < 4; ++i)
        {
            container.Add(new Container
            {
                Size = new Vector2(75, 45),
                Anchor = anchors[i],
                Origin = Anchor.TopCentre,
                Rotation = 90 * i,
                Child = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    Texture = texture,
                }
            });
        }

        return container;
    }

    private Container createTouchShadow(ISkinSource skin)
    {
        var texture = skin.GetTexture("sentakki/glow/touchhold");

        var container = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Colour = Color4.Black,
            Rotation = 45,
        };

        Anchor[] anchors = [Anchor.TopCentre, Anchor.CentreRight, Anchor.BottomCentre, Anchor.CentreLeft];

        for (int i = 0; i < 4; ++i)
        {
            container.Add(new Container
            {
                Anchor = anchors[i],
                Rotation = 90 * i,
                Size = new Vector2(75, 45),
                Origin = Anchor.TopCentre,

                Child = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(1.5f),
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    Texture = texture,
                }
            });
        }

        return container;
    }

    private void applyCustomUpdateState(DrawableHitObject hitobject, ArmedState state)
    {
        if (hitobject != drawableTouchHold)
            return;

        double initialLifetimeOffset = drawableTouchHold.HitObject.StartTime - drawableTouchHold.AnimationStartTime.Value;

        double animTime = initialLifetimeOffset * 0.8;
        double fadeTime = initialLifetimeOffset * 0.2;

        using (BeginAbsoluteSequence(drawableTouchHold.AnimationStartTime.Value))
        {
            this.FadeInFromZero(fadeTime);
            mainPiece.Delay(fadeTime).ResizeTo(90, animTime, Easing.InCirc);
        }

        using (BeginAbsoluteSequence(drawableTouchHold.HitObject.StartTime))
        {
            ProgressPiece.TransformBindableTo(ProgressPiece.ProgressBindable, 1, drawableTouchHold.HitObject.Duration);
        }
    }
}
