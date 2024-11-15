using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.Graphics
{
    [TestFixture]
    public partial class TestSceneChevronVisual : OsuGridTestScene
    {
        protected override Ruleset CreateRuleset() => new SentakkiRuleset();
        public TestSceneChevronVisual() : base(1, 2)
        {
            Cell(0).Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White
            });
            Cell(1).Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White
            });
            Cell(0).Add(new SlideChevron());
            Cell(1).Add(new DrawableChevron()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(80, 62.5f),
                Thickness = 7
            });
        }
    }
}
