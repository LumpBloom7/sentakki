// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
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

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            ActiveCursor.RelativePositionAxes = Axes.None;
            ActiveCursor.Position = e.MousePosition;
            ActiveCursor.RelativePositionAxes = Axes.Both;
            return base.OnMouseMove(e);
        }
    }
}
