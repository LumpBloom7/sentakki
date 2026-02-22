
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;

public partial class SlideSelectionBlueprint : SentakkiSelectionBlueprint<Slide, DrawableSlide>
{
    private readonly SlideBodyHighlight? slideBodyHighlight;
    private readonly SlideTapPiece slideTapHighlight;

    public override Quad SelectionQuad => slideTapHighlight.ScreenSpaceDrawQuad;
    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        => slideTapHighlight.ReceivePositionalInputAt(screenSpacePos) || DrawableObject.SlideBodies.Select(s => s.Slidepath).Any(s => s.ReceivePositionalInputAt(screenSpacePos));

    public SlideSelectionBlueprint(Slide item)
        : base(item)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        if (item.SlideInfoList.Count == 1)
            AddInternal(slideBodyHighlight = new SlideBodyHighlight(item, item.SlideInfoList[0]) { Colour = Color4.YellowGreen });

        AddInternal(slideTapHighlight = new SlideTapPiece { Colour = Color4.YellowGreen });
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

        if (slideBodyHighlight is null)
            return;

        slideBodyHighlight.UpdateFrom(DrawableObject.SlideBodies[0]);
    }
}
