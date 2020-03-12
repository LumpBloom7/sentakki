// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Framework.Graphics.Effects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;
using osu.Game.Rulesets.Maimai.Configuration;

namespace osu.Game.Rulesets.Maimai.UI
{
    [Cached]
    public class MaimaiPlayfield : Playfield
    {
        public static readonly float ringSize = 600;
        private readonly float dotSize = 20f;
        private readonly float intersectDistance = 296.5f;

        public MaimaiPlayfield()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.None;
            Size = new Vector2(ringSize + 100);
            AddRangeInternal(new Drawable[]
            {
                new GlowPiece
                {
                    Size = new Vector2(ringSize),
                    Colour = Color4.Pink,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]{
                        new CircularContainer{
                            FillAspectRatio = 1,
                            FillMode = FillMode.Fit,
                            //RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(ringSize),
                            Masking = true,
                            BorderThickness = 8.5f,
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
                        new CircularContainer{
                            FillAspectRatio = 1,
                            FillMode = FillMode.Fit,
                            //RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(ringSize),
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
                            //RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(ringSize),
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
                        new Container
                        {
                            FillAspectRatio = 1,
                            FillMode = FillMode.Fit,
                            Rotation = 22.5f,
                            //RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(ringSize),
                            Children = new Drawable[]
                            {
                                new DotPiece
                                {
                                    Size = new Vector2(dotSize),
                                    Position = new Vector2(0, -intersectDistance)

                                },
                                new DotPiece
                                {
                                    Size = new Vector2(dotSize),
                                    Position = new Vector2(0, intersectDistance)

                                },
                                new DotPiece
                                {
                                    Size = new Vector2(dotSize),
                                    Position = new Vector2(-intersectDistance, 0)

                                },
                                new DotPiece
                                {
                                    Size = new Vector2(dotSize),
                                    Position = new Vector2(intersectDistance, 0)

                                },
                            }
                        },
                        new Container
                        {
                            Rotation = -22.5f,
                            FillAspectRatio = 1,
                            FillMode = FillMode.Fit,
                            //RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(ringSize),
                            Children = new Drawable[]
                            {
                                new DotPiece
                                {
                                    Size = new Vector2(dotSize),
                                    Position = new Vector2(0, -intersectDistance)

                                },
                                new DotPiece
                                {
                                    Size = new Vector2(dotSize),
                                    Position = new Vector2(0, intersectDistance)

                                },
                                new DotPiece
                                {
                                    Size = new Vector2(dotSize),
                                    Position = new Vector2(-intersectDistance, 0)

                                },
                                new DotPiece
                                {
                                    Size = new Vector2(dotSize),
                                    Position = new Vector2(intersectDistance, 0)

                                },
                            }
                        },

                    }
                }/*.WithEffect(new GlowEffect{
                    Colour = Color4.Pink,
                    PadExtent = true,
                    Strength = 2,
                    CacheDrawnEffect = true,
                })*/,
                HitObjectContainer,
                new VisualisationContainer(),
            });
        }
        protected override GameplayCursorContainer CreateCursor() => new MaimaiCursorContainer();

        public class DotPiece : Container
        {
            public DotPiece()
            {
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
        private class VisualisationContainer : BeatSyncedContainer
        {
            private readonly float ringSize = 600;

            private LogoVisualisation visualisation;
            private readonly Bindable<bool> showVisualisation = new Bindable<bool>(true);

            [BackgroundDependencyLoader(true)]
            private void load(MaimaiRulesetConfigManager settings)
            {
                FillAspectRatio = 1;
                FillMode = FillMode.Fit;
                // RelativeSizeAxes = Axes.Both;
                Size = new Vector2(ringSize);
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;


                Child = visualisation = new LogoVisualisation
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Color4.Pink.Darken(.8f),

                };

                settings?.BindWith(MaimaiRulesetSettings.ShowVisualizer, showVisualisation);
                showVisualisation.TriggerChange();
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                visualisation.AccentColour = Color4.Pink.Darken(.8f);
            }

            protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
            {
                if (effectPoint.KiaiMode && showVisualisation.Value)
                {
                    visualisation.FadeIn(200);

                }
                else
                {
                    visualisation.FadeOut(500);
                }
            }
        }
    }
}
