using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Skinning;
using osu.Game.Rulesets.Sentakki.Skinning.Common;
using osu.Game.Rulesets.Sentakki.Skinning.Default.TouchHold;
using osu.Game.Skinning;
using osuTK;
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

    private readonly SkinnableDrawable highlight;
    public TouchHoldSelectionBlueprint(TouchHold item)
        : base(item)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        Size = new Vector2(130);

        InternalChild = highlight = new SkinnableDrawable(new SentakkiSkinComponentLookup(SentakkiSkinComponents.TouchHold), _ => new TouchHoldBody(), ConfineMode.ScaleToFit)
        {
            Alpha = 0.5f,
            Colour = Color4.YellowGreen,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.None,
        };
    }

    protected override void Update()
    {
        base.Update();

        highlight.Position = HitObject.Position;

        var drawableVisuals = DrawableObject.TouchHoldBody;

        highlight.Size = drawableVisuals.Size;
        ((IHasCopyableVisualState)drawableVisuals.Drawable).CopyVisualStateTo((IHasCopyableVisualState)highlight.Drawable);
    }
}
