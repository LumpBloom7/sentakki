using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

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
                    Font = OsuFont.Torus.With(size: 40, weight: FontWeight.SemiBold),
                    ShadowColour = Color4.Gray,
                },
            };

            LivesLeft.BindValueChanged(v =>
            {
                livesText.Text = v.NewValue.ToString();
                this.FadeColour(Color4.Red, 160).Then().FadeColour(Color4.White, 320);
                Shake();
            }, true);
        }
    }
}
