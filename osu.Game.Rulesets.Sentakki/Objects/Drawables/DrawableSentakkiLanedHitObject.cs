using osu.Framework.Allocation;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects;
using osu.Framework.Graphics;
using osu.Game.Skinning;
using osu.Game.Audio;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Judgements;
using osu.Framework.Graphics.Containers;
using System;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSentakkiLanedHitObject : DrawableSentakkiHitObject
    {
        public new SentakkiLanedHitObject HitObject => (SentakkiLanedHitObject)base.HitObject;
        private readonly PausableSkinnableSound breakSound;

        protected override float SamplePlaybackPosition => SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, HitObject.Lane).X / (SentakkiPlayfield.INTERSECTDISTANCE * 2) + .5f;

        public DrawableSentakkiLanedHitObject(SentakkiLanedHitObject hitObject)
                    : base(hitObject)
        {
            AddRangeInternal(new Drawable[]{
                breakSound = new PausableSkinnableSound(new SampleInfo("Break")),
            });
            AddInternal(scorePaddingObjects = new Container<DrawableScorePaddingObject>());
        }

        private readonly Bindable<bool> breakSoundsEnabled = new Bindable<bool>(true);

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfig)
        {
            sentakkiConfig?.BindWith(SentakkiRulesetSettings.BreakSounds, breakSoundsEnabled);
            sentakkiConfig?.BindWith(SentakkiRulesetSettings.AnimationDuration, AnimationDuration);
        }

        protected virtual bool PlayBreakSample => true;
        public override void PlaySamples()
        {
            base.PlaySamples();
            if (HitObject.Break && PlayBreakSample && breakSound != null && Result.Type == Result.Judgement.MaxResult && breakSoundsEnabled.Value)
            {
                breakSound.Balance.Value = CalculateSamplePlaybackBalance(SamplePlaybackPosition);
                breakSound.Play();
            }
        }

        private readonly Container<DrawableScorePaddingObject> scorePaddingObjects;

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            scorePaddingObjects.Clear();
        }
        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);
            if (hitObject is DrawableScorePaddingObject x)
                scorePaddingObjects.Add(x);
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            if (hitObject is ScorePaddingObject x)
                return new DrawableScorePaddingObject(x);

            return base.CreateNestedHitObject(hitObject);
        }

        protected override void ApplyResult(Action<JudgementResult> application)
        {
            base.ApplyResult(application);

            // Also give Break note score padding a judgement
            foreach (var breakObj in scorePaddingObjects)
                breakObj.ApplyResult(application);
        }
    }
}
