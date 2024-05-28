using NUnit.Framework;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.UI
{
    [TestFixture]
    public partial class TestSceneNewRing : OsuTestScene
    {
        protected override Ruleset CreateRuleset() => new SentakkiRuleset();

        private RingNote ring = null!;

        public TestSceneNewRing()
        {
            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White
            });

            Add(ring = new RingNote()
            {
                Origin = Anchor.CentreLeft,
                Anchor = Anchor.Centre,
                Hex = true,
                Size = new Vector2(105, 105),
                Colour = Color4Extensions.FromHex("FF0064"),
                Glow = true,
                Thickness = 0.24f
            });
            Add(new RingNote()
            {
                Origin = Anchor.CentreRight,
                Anchor = Anchor.Centre,
                Hex = false,
                Size = new Vector2(105, 105),
                Colour = Color4Extensions.FromHex("FF0064"),
                Thickness = 0.24f
            }
            );

            AddSliderStep<float>("Test Height", 105, 500, 105, f =>
            {
                if (ring != null) ring.Height = f;
            });
            AddSliderStep<float>("Test Rotate", 0, 360, 0, f =>
            {
                if (ring != null) ring.Rotation = f;
            });
        }
    }
}
