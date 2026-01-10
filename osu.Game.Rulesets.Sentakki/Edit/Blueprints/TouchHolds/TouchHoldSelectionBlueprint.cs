using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds;

public partial class TouchHoldSelectionBlueprint : SentakkiSelectionBlueprint<TouchHold, DrawableTouchHold>
{
    public static readonly IReadOnlyList<Color4> SELECTION_PALETTE =
    [
        Color4.White,
        Color4.White.Darken(0.5f),
        Color4.White,
        Color4.White.Darken(0.5f),
    ];

    // TouchHoldBody typically relies on colour provided by DrawableTouchHold to set its colour. Since the highlight is not tied to a DHO, we provide that dependency here.
    [Cached]
    private Bindable<IReadOnlyList<Color4>>? paletteBindable { get; set; } = new Bindable<IReadOnlyList<Color4>>(SELECTION_PALETTE);

    private readonly TouchHoldBody highlight;

    public override Quad SelectionQuad => highlight.ProgressPiece.ScreenSpaceDrawQuad;

    public TouchHoldSelectionBlueprint(TouchHold item)
        : base(item)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        InternalChild = highlight = new TouchHoldBody
        {
            Alpha = 0.5f,
            Colour = Color4.YellowGreen
        };
    }

    protected override void Update()
    {
        base.Update();

        highlight.Position = HitObject.Position;

        var drawableVisuals = DrawableObject.TouchHoldBody;

        highlight.Size = drawableVisuals.Size;
        highlight.CentrePiece.Alpha = drawableVisuals.CentrePiece.Alpha;
        highlight.CompletedCentre.Alpha = drawableVisuals.CompletedCentre.Alpha;
        highlight.ProgressPiece.ProgressBindable.Value = drawableVisuals.ProgressPiece.ProgressBindable.Value;
    }
}
