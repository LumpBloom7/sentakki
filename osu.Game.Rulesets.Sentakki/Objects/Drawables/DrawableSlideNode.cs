using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
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
        private DrawableSlideBody parentSlide => (DrawableSlideBody)ParentHitObject;

        // Used to determine the node order
        private int thisIndex;

        // Hits are only possible if this the second node before this one is hit
        // If the second node before this one doesn't exist, it is allowed as this is one of the first nodes
        protected bool IsHittable => thisIndex < 2 || parentSlide.SlideNodes[thisIndex - 2].IsHit;

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

        protected override void OnApply()
        {
            base.OnApply();
            Position = parentSlide.HitObject.SlideInfo.SlidePath.Path.PositionAt(HitObject.Progress);
            thisIndex = parentSlide.SlideNodes.IndexOf(this);
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
            if (parentSlide.HitObject != null)
            {
                if (Time.Current >= parentSlide.HitObject.StartTime)
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
                parentSlide.SlideNodes[thisIndex - 1]?.ApplyResult(application);

            base.ApplyResult(application);
        }

        // Forcefully miss this node, used when players fail to complete the slide on time.
        public void ForcefullyMiss() => ApplyResult(r => r.Type = r.Judgement.MinResult);
    }
}
