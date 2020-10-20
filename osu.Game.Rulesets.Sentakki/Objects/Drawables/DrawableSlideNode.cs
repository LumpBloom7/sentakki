using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideNode : DrawableSentakkiHitObject
    {
        private PausableSkinnableSound slideSound;

        public override bool HandlePositionalInput => true;
        public override bool DisplayResult => false;

        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;
        protected DrawableSlideBody Slide;

        public int ThisIndex;
        public DrawableSlideNode(SlideBody.SlideNode node, DrawableSlideBody slideNote)
            : base(node)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Slide = slideNote;
            RelativeSizeAxes = Axes.None;
            Position = slideNote.Slidepath.Path.PositionAt((HitObject as SlideBody.SlideNode).Progress);
            Size = new Vector2(240);
            CornerExponent = 2f;
            CornerRadius = 120;
            Masking = true;
        }

        protected override void LoadSamples()
        {
            base.LoadSamples();
            if (ThisIndex == 0)
                AddInternal(slideSound = new PausableSkinnableSound(new SampleInfo("slide")));
        }

        protected bool IsHittable => ThisIndex < 2 || Slide.SlideNodes[ThisIndex - 2].IsHit;

        private readonly Bindable<bool> playSlideSample = new Bindable<bool>(true);

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfig)
        {
            sentakkiConfig?.BindWith(SentakkiRulesetSettings.SlideSounds, playSlideSample);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (!userTriggered || Auto)
            {
                if (timeOffset > 0 && Auto)
                    ApplyResult(r => r.Type = r.Judgement.MaxResult);
                return;
            }

            if (!IsHittable)
                return;

            ApplyResult(r => r.Type = r.Judgement.MaxResult);
        }

        // Forces this object to have a result.
        public void ForcefullyMiss() => ApplyResult(r => r.Type = r.Judgement.MinResult);

        protected new void ApplyResult(Action<JudgementResult> application)
        {
            if (ThisIndex > 0)
                Slide.SlideNodes[ThisIndex - 1]?.ApplyResult(application);

            if (!Result.HasResult)
                base.ApplyResult(application);
        }

        protected override void Update()
        {
            base.Update();
            if (Time.Current >= Slide.HitObject.StartTime)
            {
                var touchInput = SentakkiActionInputManager.CurrentState.Touch;
                bool isTouched = touchInput.ActiveSources.Any(s => ReceivePositionalInputAt(touchInput.GetTouchPosition(s) ?? new Vector2(float.MinValue)));

                if (isTouched || (IsHovered && SentakkiActionInputManager.PressedActions.Any()))
                    UpdateResult(true);
            }
        }

        public override void PlaySamples()
        {
            base.PlaySamples();
            if (playSlideSample.Value && slideSound != null && Result.Type != Result.Judgement.MinResult)
            {
                slideSound.Balance.Value = CalculateSamplePlaybackBalance(SamplePlaybackPosition);
                slideSound.Play();
            }
        }
    }
}
