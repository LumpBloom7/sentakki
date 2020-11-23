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

        public override bool HandlePositionalInput => true;
        public override bool DisplayResult => false;

        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;
        protected DrawableSlideBody Slide;

        // Used to determine the node order
        private int thisIndex;

        // Hits are only possible if this the second node before this one is hit
        // If the second node before this one doesn't exist, it is allowed as this is one of the first nodes
        protected bool IsHittable => thisIndex < 2 || Slide.SlideNodes[thisIndex - 2].IsHit;

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
            thisIndex = Slide.SlideNodes.IndexOf(this);
        }

        private readonly Bindable<bool> playSlideSample = new Bindable<bool>(true);

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfig)
        {
            sentakkiConfig?.BindWith(SentakkiRulesetSettings.SlideSounds, playSlideSample);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            // Don't allow for user input if auto is enabled for touch based objects (AutoTouch mod)
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

        protected override void ApplyResult(Action<JudgementResult> application)
        {
            // Judge the previous node, because that isn't guaranteed due to the leniency;
            if (thisIndex > 0)
                Slide.SlideNodes[thisIndex - 1]?.ApplyResult(application);

            base.ApplyResult(application);
        }

        // Forcefully miss this node, used when players fail to complete the slide on time.
        public void ForcefullyMiss() => ApplyResult(r => r.Type = r.Judgement.MinResult);
    }
}
