using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;
using System.Collections.Generic;
using osu.Framework.Graphics.Lines;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using System;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class StarPiece : CompositeDrawable
    {
        private Texture starTexture;
        public StarPiece()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            starTexture = textures.Get("star");
            AddInternal(new Sprite()
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Texture = starTexture,
                Size = new Vector2(1.8f)
            });
        }
    }
}
