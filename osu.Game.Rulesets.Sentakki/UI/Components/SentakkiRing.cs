// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sentakki.Configuration;
using osuTK;
using osuTK.Graphics;
using System;

namespace osu.Game.Rulesets.Sentakki.UI.Components
{
    public class SentakkiRing : CompositeDrawable
    {
        private readonly Container spawnIndicator;

        public SentakkiRing()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = Vector2.Zero;
            Alpha = 0;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Name = "Ring",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(SentakkiPlayfield.RingSize),
                    FillAspectRatio = 1,
                    Children = new Drawable[]{
                        new CircularContainer{
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Masking = true,
                            BorderThickness = 8.35f,
                            BorderColour = Color4.Gray,
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
                            BorderColour = Color4.Gray,
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
                    Name = "Spawn indicatiors",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0
                }
            };

            // Add dots to the actual ring
            foreach (float pathAngle in SentakkiPlayfield.PathAngles)
            {
                AddInternal(new CircularContainer
                {
                    Name = "Dot",
                    Size = new Vector2(SentakkiPlayfield.DotSize),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.None,
                    Masking = true,
                    BorderColour = Color4.Gray,
                    BorderThickness = 2,
                    Position = new Vector2(-(SentakkiPlayfield.IntersectDistance * (float)Math.Cos((pathAngle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.IntersectDistance * (float)Math.Sin((pathAngle + 90f) * (float)(Math.PI / 180)))),
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
                    BorderColour = Color4.Gray,
                    BorderThickness = 2f,
                    Rotation = pathAngle,
                    Position = new Vector2(-(SentakkiPlayfield.NoteStartDistance * (float)Math.Cos((pathAngle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.NoteStartDistance * (float)Math.Sin((pathAngle + 90f) * (float)(Math.PI / 180)))),
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                });
            }
        }

        public readonly Bindable<float> RingOpacity = new Bindable<float>(1);
        public readonly Bindable<bool> NoteStartIndicators = new Bindable<bool>(false);
        private readonly Bindable<bool> diffBasedColor = new Bindable<bool>(false);
        private readonly Bindable<bool> kiaiEffect = new Bindable<bool>(true);

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager settings, OsuColour colours, DrawableSentakkiRuleset ruleset)
        {
            settings?.BindWith(SentakkiRulesetSettings.RingOpacity, RingOpacity);
            RingOpacity.BindValueChanged(opacity => this.Alpha = opacity.NewValue);

            settings?.BindWith(SentakkiRulesetSettings.ShowNoteStartIndicators, NoteStartIndicators);
            NoteStartIndicators.BindValueChanged(opacity => spawnIndicator.FadeTo(Convert.ToSingle(opacity.NewValue), 200));

            settings?.BindWith(SentakkiRulesetSettings.DiffBasedRingColor, diffBasedColor);
            diffBasedColor.BindValueChanged(enabled =>
            {
                if (enabled.NewValue)
                {
                    this.FadeColour(colours.ForDifficultyRating(ruleset?.Beatmap.BeatmapInfo.DifficultyRating ?? DifficultyRating.Normal, true), 200);
                }
                else
                {
                    this.FadeColour(Color4.White, 200);
                }
            });

            settings?.BindWith(SentakkiRulesetSettings.KiaiEffects, kiaiEffect);
        }

        protected override void LoadComplete()
        {
            NoteStartIndicators.TriggerChange();
            this.FadeTo(RingOpacity.Value, 1000, Easing.OutElasticQuarter).ScaleTo(1, 1000, Easing.OutElasticQuarter);
            diffBasedColor.TriggerChange();
        }

        public void KiaiBeat()
        {
            if (kiaiEffect.Value)
                this.ScaleTo(1.01f, 100).Then().ScaleTo(1, 100);
        }
    }
}
