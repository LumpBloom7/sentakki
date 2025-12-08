using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;

public partial class SlideBodyHighlight : CompositeDrawable
{
    private readonly StarPiece[] starPieces;
    private readonly SlideVisual slideVisual;

    public Quad SelectionQuad => slideVisual.ScreenSpaceDrawQuad;

    private readonly SlideBodyInfo slideBodyInfo;

    public SlideBodyHighlight(SlideBodyInfo slideBodyInfo)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        this.slideBodyInfo = slideBodyInfo;

        starPieces = new StarPiece[3];

        for (int i = 0; i < 3; ++i)
            starPieces[i] = new StarPiece();

        InternalChildren =
        [
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                // Slide paths are built with the assumption that it will always start from Lane 0
                // We apply a counter rotation so that the visuals line up when lane rotation is applied at a higher level.
                Rotation = -22.5f,
                Children =
                [
                    slideVisual = new SlideVisual
                    {
                    },
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = starPieces,
                    }
                ]
            }
        ];
    }

    public void UpdateFrom(DrawableSlideBody drawableObject)
    {
        for (int i = 0; i < starPieces.Length; ++i)
        {
            var localStar = starPieces[i];
            var dhoStar = drawableObject.SlideStars[i];

            localStar.Alpha = dhoStar.Alpha;
            localStar.Scale = dhoStar.Scale;
            localStar.Rotation = dhoStar.Rotation;
            localStar.Position = dhoStar.Position;
        }
    }

    public override void Show()
    {
        slideVisual.SlideBodyInfo = slideBodyInfo;
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
        slideVisual.SlideBodyInfo = null;
    }
}
