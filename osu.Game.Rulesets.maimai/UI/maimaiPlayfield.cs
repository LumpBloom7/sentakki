// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
using osu.Game.Rulesets.maimai.Configuration;

namespace osu.Game.Rulesets.maimai.UI
{
    [Cached]
    public class maimaiPlayfield : Playfield
    {
        private readonly float ringSize = .8f;
        private readonly float dotSize = 40f;
        private readonly float intersectDistance = .49f;

        public maimaiPlayfield()
        {
            AddRangeInternal(new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]{
                        new Container
                        {
                            FillAspectRatio = 1,
                            FillMode = FillMode.Fit,
                            Rotation = 22.5f,
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(ringSize),
                            Children = new Drawable[]
                            {
                                new Circle
                                {
                                    RelativeSizeAxes = Axes.None,
                                    RelativePositionAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(dotSize),
                                    Position = new Vector2(0, -intersectDistance)
                                },
                                new Circle
                                {
                                    RelativeSizeAxes = Axes.None,
                                    RelativePositionAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(dotSize),
                                    Position = new Vector2(0, intersectDistance)
                                },
                                new Circle
                                {
                                    RelativeSizeAxes = Axes.None,
                                    RelativePositionAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(dotSize),
                                    Position = new Vector2(-intersectDistance, 0)
                                },
                                new Circle
                                {
                                    RelativeSizeAxes = Axes.None,
                                    RelativePositionAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
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
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(ringSize),
                            Children = new Drawable[]
                            {
                                new Circle
                                {
                                    RelativeSizeAxes = Axes.None,
                                    RelativePositionAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(dotSize),
                                    Position = new Vector2(0, -intersectDistance)
                                },
                                new Circle
                                {
                                    RelativeSizeAxes = Axes.None,
                                    RelativePositionAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(dotSize),
                                    Position = new Vector2(0, intersectDistance)
                                },
                                new Circle
                                {
                                    RelativeSizeAxes = Axes.None,
                                    RelativePositionAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(dotSize),
                                    Position = new Vector2(-intersectDistance, 0)
                                },
                                new Circle
                                {
                                    RelativeSizeAxes = Axes.None,
                                    RelativePositionAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(dotSize),
                                    Position = new Vector2(intersectDistance, 0)
                                },
                            }
                        },
                        new CircularContainer{
                            FillAspectRatio = 1,
                            FillMode = FillMode.Fit,
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(ringSize),
                            Masking = true,
                            BorderThickness = 10,
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
                    }
                }.WithEffect(new GlowEffect{
                    Colour = Color4.Pink,
                    PadExtent = true,
                    Strength = 2,
                    CacheDrawnEffect = true,
                }),
                HitObjectContainer,
                new VisualisationContainer(),
            });
        }
        protected override GameplayCursorContainer CreateCursor() => new maimaiCursorContainer();

        private class VisualisationContainer : BeatSyncedContainer
        {
            private readonly float ringSize = 0.77f;

            private LogoVisualisation visualisation;
            private readonly Bindable<bool> showVisualisation = new Bindable<bool>(true);

            [BackgroundDependencyLoader(true)]
            private void load(maimaiRulesetConfigManager settings)
            {
                FillAspectRatio = 1;
                FillMode = FillMode.Fit;
                RelativeSizeAxes = Axes.Both;
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

                settings?.BindWith(maimaiRulesetSettings.ShowVisualizer, showVisualisation);

                showVisualisation.ValueChanged += value => { visualisation.FadeTo(value.NewValue ? 1 : 0, 500); };
                showVisualisation.TriggerChange();
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                visualisation.AccentColour = Color4.Pink.Darken(.8f);
            }

            protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
            {
                if (effectPoint.KiaiMode)
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
