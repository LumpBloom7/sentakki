using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class SentakkiCursorContainer : GameplayCursorContainer
    {
        private Sprite? cursorSprite;
        private Texture? cursorTexture;

        protected override Drawable CreateCursor() => cursorSprite = new Sprite
        {
            Origin = Anchor.Centre,
            Texture = cursorTexture,
        };

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            cursorTexture = textures.Get("SentakkiIcon.png");

            if (cursorSprite != null)
                cursorSprite.Texture = cursorTexture;
        }
    }
}
