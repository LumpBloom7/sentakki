using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Game.Online.API;
using osu.Game.Tests.Visual;
using osu.Game.Users;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests
{
    public class TestSceneOsuGame : OsuTestScene
    {
        [BackgroundDependencyLoader]
        private void load(GameHost host, OsuGameBase gameBase)
        {
            OsuGameSupporter game = new OsuGameSupporter();
            game.SetHost(host);
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                },
                game
            };
        }
        internal class OsuGameSupporter : OsuGame
        {
            public OsuGameSupporter()
            {
                API = new DummyAPIAccess();
                API.LocalUser.Value = new User
                {
                    IsSupporter = true,
                    Username = "Mai-Chan",
                    Country = new Country { FlagName = @"BE" }
                };
            }
        }
    }
}
