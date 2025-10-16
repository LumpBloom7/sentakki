using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;

public partial class SlideBodyHighlight : CompositeDrawable
{
    private readonly Container<StarPiece> stars;

    private readonly SlideVisual slideBody;

    private readonly SlideBodyInfo slideBodyInfo;

    private bool selected;

    // For simplicity, let's just take the quad of the slide visual
    public override Quad ScreenSpaceDrawQuad => slideBody.ScreenSpaceDrawQuad;

    public SlideBodyHighlight(SlideBodyInfo slideBodyInfo)
    {
        Anchor = Origin = Anchor.Centre;
        Colour = Color4.YellowGreen;
        Alpha = 1;

        InternalChildren =
        [
            slideBody = new SlideVisual(),
            stars = new Container<StarPiece>()
        ];

        const int number_of_stars = 3;

        for (int i = 0; i < number_of_stars; ++i)
            stars.Add(new StarPiece());

        this.slideBodyInfo = slideBodyInfo;
    }

    public void OnSelected()
    {
        selected = true;
        slideBody.Path = slideBodyInfo.SlidePath;
    }

    public void OnDeselected()
    {
        selected = false;
        slideBody.Free();
    }

    public void UpdateFrom(DrawableSlideBody body)
    {
        if (!selected)
            return;

        slideBody.Path = body.Slidepath.Path;

        for (int i = 0; i < body.SlideStars.Count; ++i)
        {
            stars[i].Position = body.SlideStars[i].Position;
            stars[i].Rotation = body.SlideStars[i].Rotation;
            stars[i].Scale = body.SlideStars[i].Scale;
            stars[i].Alpha = body.SlideStars[i].Alpha;
        }
    }
}
