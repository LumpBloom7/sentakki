using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI.Components
{
    public partial class SentakkiRing : Container
    {
        private readonly Container spawnIndicator;

        public SentakkiRing()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

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
                    Position = new Vector2(-(SentakkiPlayfield.INTERSECTDISTANCE * (float)Math.Cos((pathAngle + 90f) * (float)(Math.PI / 180))),
                        -(SentakkiPlayfield.INTERSECTDISTANCE * (float)Math.Sin((pathAngle + 90f) * (float)(Math.PI / 180)))),
                });

                spawnIndicator.Add(new DotPiece(size: new Vector2(16, 8))
                {
                    Rotation = pathAngle,
                    Position = new Vector2(-(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Cos((pathAngle + 90f) * (float)(Math.PI / 180))),
                        -(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Sin((pathAngle + 90f) * (float)(Math.PI / 180)))),
                });
            }
        }

        public readonly Bindable<float> RingOpacity = new Bindable<float>(1);
        public readonly Bindable<bool> NoteStartIndicators = new Bindable<bool>();
        private readonly Bindable<bool> kiaiEffect = new Bindable<bool>(true);

        [BackgroundDependencyLoader]
        private void load(SentakkiRulesetConfigManager? settings)
        {
            settings?.BindWith(SentakkiRulesetSettings.RingOpacity, RingOpacity);
            RingOpacity.BindValueChanged(opacity => Alpha = opacity.NewValue, true);

            settings?.BindWith(SentakkiRulesetSettings.ShowNoteStartIndicators, NoteStartIndicators);
            NoteStartIndicators.BindValueChanged(opacity => spawnIndicator.FadeTo(Convert.ToSingle(opacity.NewValue), 200), true);

            settings?.BindWith(SentakkiRulesetSettings.KiaiEffects, kiaiEffect);
        }

        protected override void LoadComplete()
        {
            // These usually animate in, but they shouldn't if the game was started with it already on
            spawnIndicator.FinishTransforms(true);
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
