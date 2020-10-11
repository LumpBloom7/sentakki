using osuTK;
using osu.Framework.Graphics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Bindables;
using osu.Game.Skinning;
using osu.Game.Audio;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Judgements;

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

        protected int ThisIndex;
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
        protected override void LoadComplete()
        {
            base.LoadComplete();
            ThisIndex = Slide.SlideNodes.IndexOf(this);

            OnNewResult += (DrawableHitObject hitObject, JudgementResult result) =>
            {
                hitPreviousNodes(result.Type == result.Judgement.MaxResult);
                if (result.IsHit)
                    Slide.Slidepath.Progress = (HitObject as SlideBody.SlideNode).Progress;
            };
            OnRevertResult += (DrawableHitObject hitObject, JudgementResult result) =>
            {
                Slide.Slidepath.Progress = ThisIndex > 0 ? (Slide.SlideNodes[ThisIndex - 1].HitObject as SlideBody.SlideNode).Progress : 0;
            };
        }

        protected override void LoadSamples()
        {
            base.LoadSamples();
            AddInternal(slideSound = new PausableSkinnableSound(new SampleInfo("slide")));
        }

        protected bool IsHittable => ThisIndex < 2 || Slide.SlideNodes[ThisIndex - 2].IsHit;

        private void hitPreviousNodes(bool successful = false)
        {
            foreach (var node in Slide.SlideNodes)
            {
                if (node == this) return;
                if (!node.Result.HasResult)
                    node.ForceJudgement(successful);
            }
        }

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
        public void ForceJudgement(bool successful = false) => ApplyResult(r => r.Type = successful ? r.Judgement.MaxResult : r.Judgement.MinResult);

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
            if (ThisIndex == 0 && playSlideSample.Value && slideSound != null && Result.Type != Result.Judgement.MinResult)
            {
                slideSound.Balance.Value = CalculateSamplePlaybackBalance(SamplePlaybackPosition);
                slideSound.Play();
            }
        }
    }
}
