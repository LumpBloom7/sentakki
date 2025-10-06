using NUnit.Framework;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests.UI;

[TestFixture]
public partial class TestSceneHitExplosion : OsuTestScene
{
    public TestSceneHitExplosion()
    {
        HitExplosion explosion;
        Add(explosion = new HitExplosion());
        AddStep("Explode", () =>
        {
            explosion.Explode();
        });
    }
}
