using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Skinning;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI.Components
{
    public class SentakkiRing : CompositeDrawable
    {
        private readonly Container spawnIndicator;

        public SentakkiRing()
        {
            Size = new Vector2(SentakkiPlayfield.RINGSIZE);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = Vector2.Zero;
            Alpha = 0;

            InternalChildren = new Drawable[]
            {
                new RingPiece(8),
                spawnIndicator = new Container
                {
                    Name = "Spawn indicatiors",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0
                }
            };

            // Add dots to the actual ring
            foreach (float pathAngle in SentakkiPlayfield.LANEANGLES)
            {
                AddInternal(new DotPiece
                {
                    Position = new Vector2(-(SentakkiPlayfield.INTERSECTDISTANCE * (float)Math.Cos((pathAngle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.INTERSECTDISTANCE * (float)Math.Sin((pathAngle + 90f) * (float)(Math.PI / 180)))),
                });

                spawnIndicator.Add(new DotPiece(new Vector2(16, 8))
                {
                    Rotation = pathAngle,
                    Position = new Vector2(-(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Cos((pathAngle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Sin((pathAngle + 90f) * (float)(Math.PI / 180)))),
                });
            }
        }

        public readonly Bindable<float> RingOpacity = new Bindable<float>(1);
        public readonly Bindable<bool> NoteStartIndicators = new Bindable<bool>(false);
        private readonly Bindable<ColorOption> colorOption = new Bindable<ColorOption>(ColorOption.Default);
        private readonly Bindable<bool> kiaiEffect = new Bindable<bool>(true);

        private Bindable<Skin> skin;
        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager settings, OsuColour colours, DrawableSentakkiRuleset ruleset, IAPIProvider api, SkinManager skinManager)
        {
            settings?.BindWith(SentakkiRulesetSettings.RingOpacity, RingOpacity);
            RingOpacity.BindValueChanged(opacity => Alpha = opacity.NewValue);

            settings?.BindWith(SentakkiRulesetSettings.ShowNoteStartIndicators, NoteStartIndicators);
            NoteStartIndicators.BindValueChanged(opacity => spawnIndicator.FadeTo(Convert.ToSingle(opacity.NewValue), 200));

            skin = skinManager.CurrentSkin.GetBoundCopy();
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
