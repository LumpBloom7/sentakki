using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;

public partial class SlideBodyHighlight : CompositeDrawable
{
    private readonly StarPiece[] starPieces;
    private readonly Container<SlideSegmentHighlight> slideSegments;

    private readonly SlideBodyInfo slideBodyInfo;
    private readonly Slide slide;

    private readonly IBindable<int> versionBindable;

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        => slideSegments.Children.Any(c => c.ReceivePositionalInputAt(screenSpacePos));

    public SlideBodyHighlight(Slide slide, SlideBodyInfo slideBodyInfo)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        this.slideBodyInfo = slideBodyInfo;
        this.slide = slide;

        versionBindable = slideBodyInfo.Version.GetBoundCopy();

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
                    slideSegments = new Container<SlideSegmentHighlight>
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
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

        versionBindable.BindValueChanged(_ => createSegmentHighlights(), true);
    }

    private void createSegmentHighlights()
    {
        // The segment highlights will update themselves when SlideBodyInfo changes
        // This prevents the popup from closing automatically
        // We only need to remove excess entries or add missing ones to match the current count.

        if (slideSegments.Count >= slideBodyInfo.Segments.Count)
        {
            slideSegments.RemoveRange([.. slideSegments.Children.Skip(slideBodyInfo.Segments.Count)], true);
            return;
        }

        for (int i = slideSegments.Count; i < slideBodyInfo.Segments.Count; ++i)
            slideSegments.Add(new(slide, i));


        int offset = 0;

        for (int i = 0; i < slideBodyInfo.Segments.Count; ++i)
        {
            slideSegments[0].Rotation = offset * 45;

            offset += slideBodyInfo.Segments[i].RelativeEndLane;
        }
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
}
