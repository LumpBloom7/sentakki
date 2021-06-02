using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI.Components.HitObjectLine;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests
{
    [TestFixture]
    public class TestSceneSentakkiIcon : OsuTestScene
    {
        protected override Ruleset CreateRuleset() => new SentakkiRuleset();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Add(new SentakkiRuleset.SentakkiIcon(CreateRuleset()));
        }
    }
}
