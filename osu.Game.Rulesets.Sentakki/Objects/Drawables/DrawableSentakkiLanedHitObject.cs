using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public partial class DrawableSentakkiLanedHitObject : DrawableSentakkiHitObject
    {
        public new SentakkiLanedHitObject HitObject => (SentakkiLanedHitObject)base.HitObject;

        protected override float SamplePlaybackPosition =>
            (SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, HitObject.Lane).X / (SentakkiPlayfield.INTERSECTDISTANCE * 2)) + .5f;

        private PausableSkinnableSound breakSample = null!;

        private Container<DrawableScorePaddingObject> scorePaddingObjects = null!;

        private Container<DrawableScoreBonusObject> scoreBonusObjects = null!;

        public DrawableSentakkiLanedHitObject(SentakkiLanedHitObject? hitObject)
            : base(hitObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load(SentakkiRulesetConfigManager? sentakkiConfig)
        {
            AddRangeInternal(new Drawable[]
            {
                scorePaddingObjects = new Container<DrawableScorePaddingObject>(),
                scoreBonusObjects = new Container<DrawableScoreBonusObject>(),
                breakSample = new PausableSkinnableSound(),
            });

            if (DrawableSentakkiRuleset is not null)
                AnimationDuration.BindTo(DrawableSentakkiRuleset?.AdjustedAnimDuration);

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
            switch (hitObject)
            {
                case ScorePaddingObject p:
                    return new DrawableScorePaddingObject(p);

                case ScoreBonusObject b:
                    return new DrawableScoreBonusObject(b);
            }

            return base.CreateNestedHitObject(hitObject);
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableScorePaddingObject p:
                    scorePaddingObjects.Add(p);
                    break;

                case DrawableScoreBonusObject b:
                    scoreBonusObjects.Add(b);
                    break;

                default:
                    base.AddNestedHitObject(hitObject);
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            scorePaddingObjects.Clear(false);
            scoreBonusObjects.Clear(false);
        }

        protected override void OnFree()
        {
            base.OnFree();
            breakSample.ClearSamples();
        }

        protected new void ApplyResult(HitResult hitResult)
        {
            // Judge the scoreBonus
            foreach (var bonusObject in scoreBonusObjects)
                bonusObject.TriggerResult();

            // Also give Break note score padding a judgement
            for (int i = 0; i < scorePaddingObjects.Count; ++i)
                scorePaddingObjects[^(i + 1)].ApplyResult(hitResult);

            base.ApplyResult(hitResult);
        }
    }
}
