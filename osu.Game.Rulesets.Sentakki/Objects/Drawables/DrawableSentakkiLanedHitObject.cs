using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSentakkiLanedHitObject : DrawableSentakkiHitObject
    {
        public new SentakkiLanedHitObject HitObject => (SentakkiLanedHitObject)base.HitObject;

        protected override float SamplePlaybackPosition => (SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, HitObject.Lane).X / (SentakkiPlayfield.INTERSECTDISTANCE * 2)) + .5f;

        private PausableSkinnableSound breakSample = null!;

        private Container<DrawableScorePaddingObject> scorePaddingObjects = null!;

        public DrawableSentakkiLanedHitObject(SentakkiLanedHitObject? hitObject)
                    : base(hitObject) { }

        [BackgroundDependencyLoader]
        private void load(SentakkiRulesetConfigManager? sentakkiConfig)
        {
            AddRangeInternal(new Drawable[]{
                scorePaddingObjects = new Container<DrawableScorePaddingObject>(),
                breakSample = new PausableSkinnableSound(),
            });

            sentakkiConfig?.BindWith(SentakkiRulesetSettings.AnimationDuration, AnimationDuration);
            sentakkiConfig?.BindWith(SentakkiRulesetSettings.BreakSampleVolume, breakSample.Volume);
        }

        protected override void LoadSamples()
        {
            base.LoadSamples();

            breakSample.Samples = HitObject.CreateBreakSample();
        }

        public override void PlaySamples()
        {
            base.PlaySamples();

            breakSample.Balance.Value = CalculateSamplePlaybackBalance(SamplePlaybackPosition);
            breakSample.Play();
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            if (hitObject is ScorePaddingObject x)
                return new DrawableScorePaddingObject(x);

            return base.CreateNestedHitObject(hitObject);
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);
            if (hitObject is DrawableScorePaddingObject x)
                scorePaddingObjects.Add(x);
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            scorePaddingObjects.Clear(false);
        }

        protected override void OnFree()
        {
            base.OnFree();
            breakSample.Samples = null;
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
