using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds;

public partial class TouchHoldHighlight : TouchHoldBody
{
    [Cached]
    private Bindable<IReadOnlyList<Color4>>? paletteBindable { get; set; } = new Bindable<IReadOnlyList<Color4>>([
            Color4.YellowGreen,
            Color4.YellowGreen.Darken(0.7f),
            Color4.YellowGreen,
            Color4.YellowGreen.Darken(0.7f),
        ]
    );

    public override Quad ScreenSpaceDrawQuad => ProgressPiece.ScreenSpaceDrawQuad;

    public TouchHoldHighlight()
    {
        Anchor = Origin = Anchor.Centre;
        Colour = Color4.YellowGreen;
        Alpha = 0.5f;
    }
}