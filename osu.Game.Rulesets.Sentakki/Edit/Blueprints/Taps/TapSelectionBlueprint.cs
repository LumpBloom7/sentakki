using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps;

public partial class TapSelectionBlueprint : SentakkiSelectionBlueprint<Tap, DrawableTap>
{
    private readonly TapPiece highlight;
    public override Quad SelectionQuad => DrawableObject.TapVisual.ScreenSpaceDrawQuad;

    public TapSelectionBlueprint(Tap item)
        : base(item)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        AddInternal(highlight = new TapPiece
        {
            Alpha = 0.5f,
            Colour = Color4.YellowGreen
        });
    }

    protected override void Update()
    {
        base.Update();
        Rotation = HitObject.Lane.GetRotationForLane();
        highlight.Scale = DrawableObject.TapVisual.Scale;
        highlight.Y = DrawableObject.TapVisual.Y;
    }
}
