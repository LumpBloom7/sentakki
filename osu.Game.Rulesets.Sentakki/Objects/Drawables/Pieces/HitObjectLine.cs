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
            Scale = new Vector2(.22f);
            Size = new Vector2(299);
            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            lineTexture = textures.Get("HitObjectLine");
            AddInternal(lineSprite = new Sprite()
            {
                RelativeSizeAxes = Axes.Both,
                Rotation = -45,
                Anchor = Anchor.Centre,
                Origin = Anchor.BottomLeft,
                Texture = lineTexture
            });
        }
    }
}
