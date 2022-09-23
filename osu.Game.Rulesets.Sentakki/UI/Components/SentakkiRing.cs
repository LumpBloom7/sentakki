using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Skinning.Default;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI.Components
{
    public class SentakkiRing : Container
    {
        private readonly Container spawnIndicator;

        public SentakkiRing()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = Vector2.Zero;
            Alpha = 0;

            InternalChildren = new Drawable[]
            {
                new RingPiece(thickness: 8),
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

                spawnIndicator.Add(new DotPiece(size: new Vector2(16, 8))
                {
                    Rotation = pathAngle,
                    Position = new Vector2(-(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Cos((pathAngle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Sin((pathAngle + 90f) * (float)(Math.PI / 180)))),
                });
            }
        }

        public readonly Bindable<float> RingOpacity = new Bindable<float>(1);
        public readonly Bindable<bool> NoteStartIndicators = new Bindable<bool>(false);
        private readonly Bindable<bool> kiaiEffect = new Bindable<bool>(true);

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager settings)
        {
            settings?.BindWith(SentakkiRulesetSettings.RingOpacity, RingOpacity);
            RingOpacity.BindValueChanged(opacity => Alpha = opacity.NewValue);

            settings?.BindWith(SentakkiRulesetSettings.ShowNoteStartIndicators, NoteStartIndicators);
            NoteStartIndicators.BindValueChanged(opacity => spawnIndicator.FadeTo(Convert.ToSingle(opacity.NewValue), 200));

            settings?.BindWith(SentakkiRulesetSettings.KiaiEffects, kiaiEffect);
        }

        protected override void LoadComplete()
        {
            NoteStartIndicators.TriggerChange();
            this.FadeTo(RingOpacity.Value, 1000, Easing.OutElasticQuarter).ScaleTo(1, 1000, Easing.OutElasticQuarter);
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
