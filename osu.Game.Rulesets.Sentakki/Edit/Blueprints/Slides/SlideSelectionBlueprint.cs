using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;

public partial class SlideSelectionBlueprint : SentakkiSelectionBlueprint<Slide, DrawableSlide>
{
    private readonly SlideBodyHighlight[] slideBodyHighlights;
    private readonly SlideTapPiece slideTapHighlight;

    public override Quad SelectionQuad => slideTapHighlight.ScreenSpaceDrawQuad;

    public SlideSelectionBlueprint(Slide item)
        : base(item)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        slideBodyHighlights = [.. item.SlideBodies.Select(sb => new SlideBodyHighlight(sb.SlideBodyInfo))];

        InternalChildren =
        [
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0.5f,
                Colour = Color4.YellowGreen,
                Children = slideBodyHighlights
            },
            slideTapHighlight = new SlideTapPiece
            {
                Alpha = 0.5f,
                Colour = Color4.YellowGreen,
            },
        ];
    }

    protected override void Update()
    {
        base.Update();

        Rotation = HitObject.Lane.GetRotationForLane();

        SlideTapPiece tapVisual = (SlideTapPiece)DrawableObject.SlideTaps.Child.TapVisual;

        slideTapHighlight.Stars.Rotation = tapVisual.Stars.Rotation;
        slideTapHighlight.SecondStar.Alpha = tapVisual.SecondStar.Alpha;
        slideTapHighlight.Scale = tapVisual.Scale;
        slideTapHighlight.Y = tapVisual.Y;

        for (int i = 0; i < slideBodyHighlights.Length; ++i)
            slideBodyHighlights[i].UpdateFrom(DrawableObject.SlideBodies[i]);
    }
}
