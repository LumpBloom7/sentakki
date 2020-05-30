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
using osu.Game.Online.API;
using osu.Game.Users;
using osu.Game.Skinning;

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
                    Size = new Vector2(SentakkiPlayfield.RINGSIZE),
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
            foreach (float pathAngle in SentakkiPlayfield.PATHANGLES)
            {
                AddInternal(new CircularContainer
                {
                    Name = "Dot",
                    Size = new Vector2(SentakkiPlayfield.DOTSIZE),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.None,
                    Masking = true,
                    BorderColour = Color4.Gray,
                    BorderThickness = 2,
                    Position = new Vector2(-(SentakkiPlayfield.INTERSECTDISTANCE * (float)Math.Cos((pathAngle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.INTERSECTDISTANCE * (float)Math.Sin((pathAngle + 90f) * (float)(Math.PI / 180)))),
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
                    Position = new Vector2(-(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Cos((pathAngle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Sin((pathAngle + 90f) * (float)(Math.PI / 180)))),
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                });
            }
        }

        public readonly Bindable<float> RingOpacity = new Bindable<float>(1);
        public readonly Bindable<bool> NoteStartIndicators = new Bindable<bool>(false);
        private readonly Bindable<ColorOption> colorOption = new Bindable<ColorOption>(ColorOption.Default);
        private readonly Bindable<bool> kiaiEffect = new Bindable<bool>(true);

        private Bindable<User> user;
        private Bindable<Skin> skin;
        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager settings, OsuColour colours, DrawableSentakkiRuleset ruleset, IAPIProvider api, SkinManager skinManager)
        {
            settings?.BindWith(SentakkiRulesetSettings.RingOpacity, RingOpacity);
            RingOpacity.BindValueChanged(opacity => Alpha = opacity.NewValue);

            settings?.BindWith(SentakkiRulesetSettings.ShowNoteStartIndicators, NoteStartIndicators);
            NoteStartIndicators.BindValueChanged(opacity => spawnIndicator.FadeTo(Convert.ToSingle(opacity.NewValue), 200));

            user = api.LocalUser.GetBoundCopy();
            skin = skinManager.CurrentSkin.GetBoundCopy();

            user.ValueChanged += _ => colorOption.TriggerChange();
            skin.BindValueChanged(_ => colorOption.TriggerChange(), true);

            settings?.BindWith(SentakkiRulesetSettings.RingColor, colorOption);
            colorOption.BindValueChanged(option =>
            {
                if (option.NewValue == ColorOption.Default)
                    this.FadeColour(Color4.White, 200);
                else if (option.NewValue == ColorOption.Difficulty)
                    this.FadeColour(colours.ForDifficultyRating(ruleset?.Beatmap.BeatmapInfo.DifficultyRating ?? DifficultyRating.Normal, true), 200);
                else if (option.NewValue == ColorOption.Skin)
                    this.FadeColour(skin.Value.GetConfig<GlobalSkinColours, Color4>(GlobalSkinColours.MenuGlow)?.Value ?? Color4.White, 200);
            });

            settings?.BindWith(SentakkiRulesetSettings.KiaiEffects, kiaiEffect);
        }

        protected override void LoadComplete()
        {
            NoteStartIndicators.TriggerChange();
            this.FadeTo(RingOpacity.Value, 1000, Easing.OutElasticQuarter).ScaleTo(1, 1000, Easing.OutElasticQuarter);
            colorOption.TriggerChange();
        }

        public void KiaiBeat()
        {
            if (kiaiEffect.Value)
            {
                FinishTransforms();
                this.ScaleTo(1.01f, 100).Then().ScaleTo(1, 100);
            }
        }
    }
}
