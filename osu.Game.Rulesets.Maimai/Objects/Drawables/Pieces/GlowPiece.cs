// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Maimai.Objects.Drawables.Pieces
{
    public class GlowPiece : Container
    {
        private readonly CircularContainer glow;
        public GlowPiece()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Padding = new MarginPadding(1);
            Child = glow = new CircularContainer{
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                EdgeEffect = new EdgeEffectParameters{
                    Hollow = true,
                    Type = EdgeEffectType.Glow,
                    Radius = 15,
                    Colour = Color4.White,
                }
            };
        }

        protected override void LoadComplete(){
            glow.EdgeEffect = new EdgeEffectParameters{
                Hollow = true,
                Type = EdgeEffectType.Glow,
                Radius = 15,
                Colour = Colour,
            };
        }
    }
}
