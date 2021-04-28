using NUnit.Framework;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests.UI
{
    [TestFixture]
    public class TestSceneHitExplosion : OsuTestScene
    {
        private readonly HitExplosion explosion;
        public TestSceneHitExplosion()
        {
            Add(explosion = new HitExplosion());
            AddStep("Explode", () =>
            {
                explosion.Explode();
            });
        }
    }
}
