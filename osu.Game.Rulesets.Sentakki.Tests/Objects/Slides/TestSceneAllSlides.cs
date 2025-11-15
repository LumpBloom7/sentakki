using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects.Slides;

[TestFixture]
public partial class TestSceneAllSlides : OsuTestScene
{
    protected override Ruleset CreateRuleset() => new SentakkiRuleset();

    private int id;

    private readonly SlideBodyInfo slideBodyInfo = new SlideBodyInfo();
    private readonly Container nodes;

    [Cached]
    private readonly DrawablePool<SlideChevron> chevronPool;

    public TestSceneAllSlides()
    {
        Add(chevronPool = new DrawablePool<SlideChevron>(62));

        Add(new SentakkiRing
        {
            RelativeSizeAxes = Axes.None,
            Size = new Vector2(SentakkiPlayfield.RINGSIZE)
        });
        Add(new SlideVisual
        {
            Path = slideBodyInfo
        });

        AddSliderStep("Path ID", 0, SlidePaths.VALID_CONVERT_PATHS.Count - 1, 0, p =>
        {
            id = p;
            RefreshSlide();
        });

        Add(nodes = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        });
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        RefreshSlide();
    }

    protected void RefreshSlide()
    {
        slideBodyInfo.Segments = [SlidePaths.VALID_CONVERT_PATHS[id].Segment];
        nodes.Clear();

        foreach (var node in slideBodyInfo.SegmentPaths.SelectMany(s => s.ControlPoints))
        {
            nodes.Add(new CircularContainer
            {
                Size = new Vector2(10),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Position = node.Position,
                Masking = true,
                Child = new Box
                {
                    Colour = Color4.Green,
                    RelativeSizeAxes = Axes.Both
                }
            });
        }
    }
}
