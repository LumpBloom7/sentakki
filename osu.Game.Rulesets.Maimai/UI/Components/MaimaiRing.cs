// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Effects;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Rulesets.Maimai.Configuration;
using osuTK;
using osuTK.Graphics;
using System;

namespace osu.Game.Rulesets.Maimai.UI.Components
{
    public class MaimaiRing : CompositeDrawable
    {
        public readonly BufferedContainer hitBlur;
        private readonly Container ring;
        private readonly Container spawnIndicator;
        private readonly Container simpleRing = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Size = new Vector2(MaimaiPlayfield.RingSize + 10),
            FillAspectRatio = 1,
            FillMode = FillMode.Fit,

            Child = new CircularContainer
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Masking = true,
                BorderThickness = 18,
                BorderColour = Color4.White,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    AlwaysPresent = true,
                },
            },
        };

        public MaimaiRing()
        {
            // Add dots to the simple ring used for the blur
            foreach (float pathAngle in MaimaiPlayfield.PathAngles)
            {
                simpleRing.Add(new Circle
                {
                    Size = new Vector2(30),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.None,
                    Masking = true,
                    Position = new Vector2(-(MaimaiPlayfield.IntersectDistance * (float)Math.Cos((pathAngle + 90f) * (float)(Math.PI / 180))), -(MaimaiPlayfield.IntersectDistance * (float)Math.Sin((pathAngle + 90f) * (float)(Math.PI / 180)))),
                });
            }
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                hitBlur = simpleRing.WithEffect(new BlurEffect{
                    Sigma = new Vector2(5),
                    CacheDrawnEffect = true,
                    Colour = Color4.Fuchsia,
                    PadExtent = true,
                }),
                ring = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(MaimaiPlayfield.RingSize),
                    FillAspectRatio = 1,
                    FillMode = FillMode.Fit,
                    Children = new Drawable[]{
                        new CircularContainer{
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Masking = true,
                            BorderThickness = 8.35f,
                            BorderColour = Color4.White.Darken(1),
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
                        new Container{
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(1),
                            Child = new CircularContainer{
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Masking = true,
                                BorderThickness = 6,
                                BorderColour = Color4.White,
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                    AlwaysPresent = true,
                                },
                            },
                        },
                        new CircularContainer{
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Masking = true,
                            BorderThickness = 2,
                            BorderColour = Color4.White.Darken(1),
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true,
                            },
                        },
                    }
                },
                spawnIndicator = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };

            // Add dots to the actual ring
            foreach (float pathAngle in MaimaiPlayfield.PathAngles)
            {
                ring.Add(new CircularContainer
                {
                    Size = new Vector2(MaimaiPlayfield.DotSize),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.None,
                    Masking = true,
                    BorderColour = Color4.White.Darken(1),
                    BorderThickness = 2,
                    Position = new Vector2(-(MaimaiPlayfield.IntersectDistance * (float)Math.Cos((pathAngle + 90f) * (float)(Math.PI / 180))), -(MaimaiPlayfield.IntersectDistance * (float)Math.Sin((pathAngle + 90f) * (float)(Math.PI / 180)))),
                    Child = new Box
                    {
                        AlwaysPresent = true,
                        RelativeSizeAxes = Axes.Both,
                    }
                });

                spawnIndicator.Add(new CircularContainer
                {
                    Size = new Vector2(16, 8),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    BorderColour = Color4.White.Darken(1),
                    BorderThickness = 2f,
                    Rotation = pathAngle,
                    Position = new Vector2(-(MaimaiPlayfield.NoteStartDistance * (float)Math.Cos((pathAngle + 90f) * (float)(Math.PI / 180))), -(MaimaiPlayfield.NoteStartDistance * (float)Math.Sin((pathAngle + 90f) * (float)(Math.PI / 180)))),
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                });
            }
            flash();
        }

        Bindable<float> ringOpacity = new Bindable<float>(1);
        Bindable<bool> noteStartIndicators = new Bindable<bool>(false);

        [BackgroundDependencyLoader(true)]
        private void load(MaimaiRulesetConfigManager settings)
        {
            settings?.BindWith(MaimaiRulesetSettings.RingOpacity, ringOpacity);
            ringOpacity.BindValueChanged(opacity => this.Alpha = opacity.NewValue);
            ringOpacity.TriggerChange();

            settings?.BindWith(MaimaiRulesetSettings.ShowNoteStartIndicators, noteStartIndicators);
            noteStartIndicators.BindValueChanged(opacity => spawnIndicator.Alpha = Convert.ToSingle(opacity.NewValue));
            noteStartIndicators.TriggerChange();
        }

        public void flash()
        {
            hitBlur.FadeIn(50).Then().FadeOut(200);
        }
    }
}
