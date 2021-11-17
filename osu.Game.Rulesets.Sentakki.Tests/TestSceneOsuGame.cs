using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Tests.Visual;
using osu.Game.Users;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests
{
    public class TestSceneOsuGame : OsuTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                },
            };
            AddGame(new OsuGameSupporter());
        }

        internal class OsuGameSupporter : OsuGame
        {
            public OsuGameSupporter()
            {
                API = new DummyAPIAccess();
                Bindable<APIUser> testUser = new Bindable<APIUser>(new APIUser
                {
                    IsSupporter = true,
                    Username = "Mai-Chan",
                    Country = new Country { FlagName = @"BE" }
                });
                API.LocalUser.BindTo(testUser);
            }
        }
    }
}
