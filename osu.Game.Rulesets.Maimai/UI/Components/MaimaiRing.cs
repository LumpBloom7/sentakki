// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;
using osuTK.Graphics;
using System;

namespace osu.Game.Rulesets.Maimai.UI.Components
{
    public class MaimaiRing : CompositeDrawable
    {
        public MaimaiRing()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                new GlowPiece(),
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]{
                        new CircularContainer{
                            FillAspectRatio = 1,
                            FillMode = FillMode.Fit,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(MaimaiPlayfield.RingSize),
                            Masking = true,
                            BorderThickness = 6,
                            BorderColour = Color4.White,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                    AlwaysPresent = true,
                                },
                            }
                        },
                        new CircularContainer{
                            FillAspectRatio = 1,
                            FillMode = FillMode.Fit,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(MaimaiPlayfield.RingSize),
                            Masking = true,
                            BorderThickness = 3,
                            BorderColour = Color4.Black,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                    AlwaysPresent = true,
                                },
                            }
                        },
                    }
                },
            };
            foreach (float pathAngle in MaimaiPlayfield.PathAngles)
                AddInternal(new DotPiece
                {
                    Position = new Vector2(-(MaimaiPlayfield.IntersectDistance * (float)Math.Cos((pathAngle + 90f) * (float)(Math.PI / 180))), -(MaimaiPlayfield.IntersectDistance * (float)Math.Sin((pathAngle + 90f) * (float)(Math.PI / 180)))),
                });
        }

        public class DotPiece : Container
        {
            public DotPiece()
            {
                Size = new Vector2(MaimaiPlayfield.DotSize);
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                RelativeSizeAxes = Axes.None;
                Children = new Drawable[] {
                    new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new CircularContainer{
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Masking = true,
                        BorderColour = Color4.Black,
                        BorderThickness = 3,
                        Child = new Box
                        {
                            AlwaysPresent = true,
                            Alpha = 0,
                            RelativeSizeAxes = Axes.Both,
                        }
                    },
                };
            }
        }

        public class GlowPiece : Container
        {
            public GlowPiece()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Size = new Vector2(MaimaiPlayfield.RingSize);
                Colour = Color4.Pink;
                //RelativeSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Child = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1.28125f),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = textures.Get("Gameplay/osu/ring-glow"),
                    Blending = BlendingParameters.Additive,
                    Alpha = 0.5f
                };
            }
        }
    }
}
