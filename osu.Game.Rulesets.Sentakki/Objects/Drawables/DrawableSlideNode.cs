using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideNode : DrawableSentakkiHitObject
    {
        public new SlideBody.SlideNode HitObject => (SlideBody.SlideNode)base.HitObject;
        //private PausableSkinnableSound slideSound;

        public override bool HandlePositionalInput => true;
        public override bool DisplayResult => false;

        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;
        protected DrawableSlideBody Slide;

        public int ThisIndex;

        public DrawableSlideNode() : this(null) { }
        public DrawableSlideNode(SlideBody.SlideNode node)
            : base(node)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.None;
            Size = new Vector2(240);
            CornerExponent = 2f;
            CornerRadius = 120;
            Masking = true;
        }

        protected override void OnParentReceived(DrawableHitObject parent)
        {
            base.OnParentReceived(parent);
            Slide = (DrawableSlideBody)parent;
            Position = Slide.HitObject.SlideInfo.SlidePath.Path.PositionAt(HitObject.Progress);
            ThisIndex = Slide.SlideNodes.IndexOf(this);
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
            if (Slide.HitObject != null)
            {
                if (Time.Current >= Slide.HitObject.StartTime)
                {
                    var touchInput = SentakkiActionInputManager.CurrentState.Touch;
                    bool isTouched = touchInput.ActiveSources.Any(s => ReceivePositionalInputAt(touchInput.GetTouchPosition(s) ?? new Vector2(float.MinValue)));

                    if (isTouched || (IsHovered && SentakkiActionInputManager.PressedActions.Any()))
                        UpdateResult(true);
                }
            }
        }
    }
}
