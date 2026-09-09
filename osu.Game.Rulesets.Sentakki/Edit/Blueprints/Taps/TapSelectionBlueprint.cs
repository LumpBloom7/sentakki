using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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

    protected override bool AlwaysShowWhenSelected => true;

    public TapSelectionBlueprint(Tap item)
        : base(item)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        InternalChild = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Size = new Vector2(TapRing.CIRCLE_RADIUS * 2),

            Child = highlight = new SkinnableDrawable(new SentakkiSkinComponentLookup(SentakkiSkinComponents.Tap), _ => new TapRing(), ConfineMode.ScaleToFit)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Colour = Color4.YellowGreen
            }
        };
    }

    protected override void Update()
    {
        base.Update();

        InternalChild.Rotation = HitObject.Lane.GetRotationForLane();
        highlight.Scale = DrawableObject.Scale;
        highlight.Y = DrawableObject.Y;
    }
}
