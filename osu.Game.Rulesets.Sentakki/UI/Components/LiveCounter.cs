using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osu.Game.Graphics.Containers;

namespace osu.Game.Rulesets.Sentakki.UI.Components
{
    public class LiveCounter : ShakeContainer
    {
        public BindableInt LivesLeft = new BindableInt(300);

        private OsuSpriteText livesText;

        public LiveCounter()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            ShakeMagnitude = 4;

            InternalChildren = new Drawable[]{

                livesText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "",
                    Font = OsuFont.Torus.With(size: 40, weight: FontWeight.SemiBold),
                    Shadow = true,
                    ShadowColour = Color4.Black,
                },
            };

            LivesLeft.BindValueChanged(v =>
            {
                livesText.Text = v.NewValue.ToString();
                livesText.FadeColour(Color4.Red, 160).Then().FadeColour(Color4.White, 320);
                Shake();
            }, true);
        }
    }
}
