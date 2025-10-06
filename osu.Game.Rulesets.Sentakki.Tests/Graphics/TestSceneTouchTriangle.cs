using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Touches;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.Graphics;

[TestFixture]
public partial class TestSceneTouchTriangle : OsuGridTestScene
{
    protected override Ruleset CreateRuleset() => new SentakkiRuleset();

    public TestSceneTouchTriangle()
        : base(1, 2)
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
        Cell(0).Add(new TouchPiece());
        Cell(1).Add(new TouchPiece
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        });
    }
}
