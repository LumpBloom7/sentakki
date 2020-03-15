// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace osu.Game.Rulesets.Maimai.Objects.Drawables.Pieces
{
    public class GlowPiece : Container
    {
        public GlowPiece()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Child = new Sprite
            {
                Size = new Vector2(102.5f),
                RelativeSizeAxes = Axes.None,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Texture = textures.Get("Gameplay/osu/ring-glow"),
                Blending = BlendingParameters.Additive,
                Alpha = 0.5f
            };
        }
    }
}
