using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges;
using osu.Framework.Utils;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class SentakkiCursorContainer : GameplayCursorContainer
    {
        private Sprite cursorSprite;
        private Texture cursorTexture;

        protected override Drawable CreateCursor() => cursorSprite = new Sprite
        {
            Scale = new Vector2(0.3f),
            Origin = Anchor.Centre,
            Texture = cursorTexture,
        };

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            cursorTexture = textures.Get("Icon2");

            if (cursorSprite != null)
                cursorSprite.Texture = cursorTexture;
        }

        private Vector2? lastPosition;
        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (!(lastPosition.HasValue && Precision.AlmostEquals(e.ScreenSpaceMousePosition, lastPosition.Value)))
            {
                Show();
                lastPosition = e.ScreenSpaceMousePosition;
            }
            ActiveCursor.RelativePositionAxes = Axes.None;
            ActiveCursor.Position = e.MousePosition;
            ActiveCursor.RelativePositionAxes = Axes.Both;
            return false;
        }
        protected override void OnTouchMove(TouchMoveEvent e)
        {
            base.OnTouchMove(e);
            Hide();
        }
    }
}
