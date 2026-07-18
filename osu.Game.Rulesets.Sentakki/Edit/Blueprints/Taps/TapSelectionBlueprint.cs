using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Skinning;
using osu.Game.Rulesets.Sentakki.Skinning.Default;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps;

public partial class TapSelectionBlueprint : SentakkiSelectionBlueprint<Tap, DrawableTap>
{
    private readonly SkinnableDrawable highlight;
    public override Quad SelectionQuad => highlight.ScreenSpaceDrawQuad;

    public TapSelectionBlueprint(Tap item)
        : base(item)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        AddInternal(highlight = new ProxyableSkinnableDrawable(new SentakkiSkinComponentLookup(SentakkiSkinComponents.Tap), _ => new TapPiece())
        {
            RelativeSizeAxes = Axes.None,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Size = new Vector2(DrawableTap.CIRCLE_RADIUS * 2),
            Alpha = 0.5f,
            Colour = Color4.YellowGreen
        });
    }

    protected override void Update()
    {
        base.Update();
        Rotation = HitObject.Lane.GetRotationForLane();
        highlight.Scale = DrawableObject.Scale;
        highlight.Y = DrawableObject.Y;
    }
}
