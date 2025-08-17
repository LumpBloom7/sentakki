using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds;

public partial class TouchHoldCompletedCentre : CompositeDrawable
{
    private readonly TouchHoldCircularProgress[] progressParts;

    [Resolved]
    private Bindable<IReadOnlyList<Color4>>? paletteBindable { get; set; }

    public TouchHoldCompletedCentre()
    {
        Origin = Anchor.Centre;
        Anchor = Anchor.Centre;
        Size = new Vector2(80);
        Masking = true;
        CornerRadius = 20;
        Rotation = 45;
        Alpha = 0;
        EdgeEffect = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Colour = Color4.Black,
            Radius = 10f,
        };
        InternalChildren =
        [
            new Container
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(2),
                Rotation = -45f,
                Children = progressParts =
                [
                    new TouchHoldCircularProgress
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        InnerRadius = 1,
                        RelativeSizeAxes = Axes.Both,
                        Progress = 0.25,
                    },
                    new TouchHoldCircularProgress
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        InnerRadius = 1,
                        RelativeSizeAxes = Axes.Both,
                        Progress = 0.25,
                        Rotation = 90
                    },
                    new TouchHoldCircularProgress
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        InnerRadius = 1,
                        RelativeSizeAxes = Axes.Both,
                        Progress = 0.25,
                        Rotation = 180
                    },
                    new TouchHoldCircularProgress
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        InnerRadius = 1,
                        RelativeSizeAxes = Axes.Both,
                        Progress = 0.25,
                        Rotation = 270
                    },
                ]
            },
        ];
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        paletteBindable?.BindValueChanged(p =>
        {
            for (int i = 0; i < progressParts.Length; ++i)
                progressParts[i].AccentColour = paletteBindable.Value[i];
        }, true);
    }
}
