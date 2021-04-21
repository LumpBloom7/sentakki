using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests.UI
{
    [TestFixture]
    public class TestSceneHitExplosion : OsuTestScene
    {
        private HitExplosion explosion;
        public TestSceneHitExplosion()
        {
            //Add(new TapPiece());
            Add(explosion = new HitExplosion());
            AddStep("Explode", () =>
            {
                explosion.Animate();
            });
        }
    }
}
