// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;
using System;

namespace osu.Game.Rulesets.Sentakki.UI
{
    [Cached]
    public class SentakkiPlayfield : Playfield, IRequireHighFrequencyMousePosition
    {
        private readonly JudgementContainer<DrawableSentakkiJudgement> judgementLayer;

        private readonly SentakkiRing ring;
        public BindableNumber<int> RevolutionDuration = new BindableNumber<int>(0);

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public static readonly float RingSize = 600;
        public static readonly float DotSize = 20f;
        public static readonly float IntersectDistance = 296.5f;
        public static readonly float NoteStartDistance = 66f;

        public static readonly float[] PathAngles =
            {
                22.5f,
                67.5f,
                112.5f,
                157.5f,
                202.5f,
                247.5f,
                292.5f,
                337.5f
            };

        public SentakkiPlayfield()
        {
            RevolutionDuration.BindValueChanged(b =>
            {
                if (b.NewValue != 0) this.Spin(b.NewValue * 1000, RotationDirection.Clockwise).Then().Loop();
            });

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.None;
            Size = new Vector2(600);
            AddRangeInternal(new Drawable[]
            {
                new VisualisationContainer(),
                ring = new SentakkiRing(),
                HitObjectContainer,
                judgementLayer = new JudgementContainer<DrawableSentakkiJudgement>
                {
                    RelativeSizeAxes = Axes.Both,
                },
            });
        }

        protected override GameplayCursorContainer CreateCursor() => new SentakkiCursorContainer();

        public override void Add(DrawableHitObject h)
        {
            base.Add(h);

            var obj = (DrawableSentakkiHitObject)h;

            obj.OnNewResult += onNewResult;
        }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (!judgedObject.DisplayResult || !DisplayJudgements.Value)
                return;

            var sentakkiObj = (DrawableSentakkiHitObject)judgedObject;

            var b = sentakkiObj.HitObject.Angle + 90;
            var a = b *= (float)(Math.PI / 180);
            DrawableSentakkiJudgement explosion;
            switch (judgedObject)
            {
                case DrawableTouchHold TH:
                    explosion = new DrawableSentakkiJudgement(result, sentakkiObj)
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                    };
                    break;

                default:
                    explosion = new DrawableSentakkiJudgement(result, sentakkiObj)
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Position = new Vector2(-(240 * (float)Math.Cos(a)), -(240 * (float)Math.Sin(a))),
                        Rotation = sentakkiObj.HitObject.Angle,
                    };
                    break;
            }

            judgementLayer.Add(explosion);

            if (result.IsHit && judgedObject.HitObject.Kiai)
                ring.KiaiBeat();
        }

        protected override void LoadComplete()
        {
            RevolutionDuration.TriggerChange();
            base.LoadComplete();
        }

        private class VisualisationContainer : BeatSyncedContainer
        {
            private LogoVisualisation visualisation;
            private readonly Bindable<bool> kiaiEffect = new Bindable<bool>(true);
            private readonly Bindable<bool> diffBasedColor = new Bindable<bool>(false);

            [BackgroundDependencyLoader(true)]
            private void load(SentakkiRulesetConfigManager settings, OsuColour colours, DrawableSentakkiRuleset ruleset)
            {
                FillAspectRatio = 1;
                FillMode = FillMode.Fit;
                RelativeSizeAxes = Axes.Both;
                Size = new Vector2(.99f);
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Child = visualisation = new LogoVisualisation
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
                settings?.BindWith(SentakkiRulesetSettings.KiaiEffects, kiaiEffect);
                kiaiEffect.TriggerChange();

                settings?.BindWith(SentakkiRulesetSettings.DiffBasedRingColor, diffBasedColor);
                diffBasedColor.BindValueChanged(enabled =>
                {
                    if (enabled.NewValue)
                        visualisation.FadeColour(colours.ForDifficultyRating(ruleset?.Beatmap.BeatmapInfo.DifficultyRating ?? DifficultyRating.Normal, true), 200);
                    else
                        visualisation.FadeColour(Color4.White, 200);
                });
            }

            protected override void LoadComplete()
            {
                diffBasedColor.TriggerChange();
            }

            protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
            {
                if (effectPoint.KiaiMode && kiaiEffect.Value)
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
