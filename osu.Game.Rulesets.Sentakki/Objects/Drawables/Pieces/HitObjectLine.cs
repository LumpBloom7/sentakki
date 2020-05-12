using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using System.Collections.Generic;
using osu.Framework.Graphics.Lines;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using System;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class HitObjectLine : CompositeDrawable
    {
        private Sprite lineSprite;
        private Texture lineTexture;
        public HitObjectLine()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            lineTexture = textures.Get("HitObjectLine");
            AddInternal(lineSprite = new Sprite()
            {
                Size = new Vector2(299),
                Scale = new Vector2(.22f),
                Rotation = -45,
                Anchor = Anchor.Centre,
                Origin = Anchor.BottomLeft,
                Texture = lineTexture
            });
        }

        public void UpdateVisual(double progress)
        {
            float newSize = .22f + (float)(.78f * progress);
            lineSprite.Scale = new Vector2(newSize);
        }
    }
}
