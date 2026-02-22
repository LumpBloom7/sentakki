using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;


public partial class SlideSegmentHighlight : CompositeDrawable
{
    private Slide slide;
    private int segmentIndex;

    private IBindable<int> versionBindable;

    private SlideBodyInfo slideBodyInfo => slide.SlideInfoList[0];
    private SlideSegment segment => slideBodyInfo.Segments[segmentIndex];

    private SlideVisual visual;

    private IReadOnlyList<Drawable> additionalHoverPoints;

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        => additionalHoverPoints.Any(d => d.ReceivePositionalInputAt(screenSpacePos)) || visual.ReceivePositionalInputAt(screenSpacePos);

    public SlideSegmentHighlight(Slide slide, int segmentIndex)
    {
        this.slide = slide;
        this.segmentIndex = segmentIndex;

        versionBindable = slideBodyInfo.Version.GetBoundCopy();

        InternalChildren = [
            visual = new SlideVisual(){
                SlideBodyInfo = new SlideBodyInfo{
                    Segments = [segment]
                }
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Rotation = 22.5f,
                Children = additionalHoverPoints = [
                    new Circle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 0,
                        Size = new Vector2(60),
                        Y = -SentakkiPlayfield.INTERSECTDISTANCE,
                        AlwaysPresent = true,
                    },
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Rotation = segment.RelativeEndLane * 45f,
                        Child =  new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            Size = new Vector2(60),
                            Y = -SentakkiPlayfield.INTERSECTDISTANCE,
                            AlwaysPresent = true,
                        }
                    }
                ]
            },
        ];
    }

    protected override bool OnHover(HoverEvent e)
    {
        Colour = Color4.Orange;
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        Colour = Color4.White;
    }
}
