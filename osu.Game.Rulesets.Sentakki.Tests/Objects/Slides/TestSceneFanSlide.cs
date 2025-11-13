using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects.Slides;

[TestFixture]
public partial class TestSceneFanSlide : OsuTestScene
{
    protected override Ruleset CreateRuleset() => new SentakkiRuleset();

    [Cached]
    private readonly DrawablePool<SlideChevron> chevronPool = null!;

    private readonly SlideVisual slide;

    public TestSceneFanSlide()
    {
        Add(chevronPool = new DrawablePool<SlideChevron>(62));

        Add(new SentakkiRing
        {
            RelativeSizeAxes = Axes.None,
            Size = new Vector2(SentakkiPlayfield.RINGSIZE)
        });

        Add(slide = new SlideVisual
        {
            Path = new SlideBodyInfo
            {
                Segments = [new SlideSegment(PathShapes.Fan, 4, false)]
            }
        });

        AddSliderStep("Progress", 0.0f, 1.0f, 0.0f, p =>
        {
            slide.Progress = p;
            slide.UpdateChevronVisibility();
        });

        AddSliderStep("Rotation", 0.0f, 360f, 22.5f, p =>
        {
            slide.Rotation = p;
        });

        AddStep("Perform entry animation", () => slide.PerformEntryAnimation(1000));
        AddWaitStep("Wait for transforms", 5);

        AddStep("Perform exit animation", () => slide.PerformExitAnimation(1000));
        AddWaitStep("Wait for transforms", 5);
    }
}
