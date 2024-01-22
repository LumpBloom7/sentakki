using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Rulesets.Judgements;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public partial class DrawableSlideCheckpointNode : DrawableSentakkiHitObject
    {
        public new SlideCheckpoint.CheckpointNode HitObject => (SlideCheckpoint.CheckpointNode)base.HitObject;

        private DrawableSlideCheckpoint checkpoint => (DrawableSlideCheckpoint)ParentHitObject;

        // We need this to be alive as soon as the parent slide note is alive
        // This is to ensure reverts are still possible during edge case situation (eg. 0 duration slide)
        protected override bool ShouldBeAlive => true;

        public override bool HandlePositionalInput => true;
        public override bool DisplayResult => false;

        private SentakkiInputManager sentakkiActionInputManager = null!;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= ((SentakkiInputManager)GetContainingInputManager());

        public DrawableSlideCheckpointNode()
            : this(null)
        {
        }

        public DrawableSlideCheckpointNode(SlideCheckpoint.CheckpointNode? node)
            : base(node)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.None;
            Size = new Vector2(200);
            CornerExponent = 2f;
            CornerRadius = 100;
        }

        protected override void OnApply()
        {
            base.OnApply();
            Position = HitObject.Position;
        }

        protected override void Update()
        {
            base.Update();
            if (Judged || !checkpoint.IsHittable)
                return;

            if (checkForTouchInput() || (IsHovered && SentakkiActionInputManager.PressedActions.Any()))
                UpdateResult(true);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            // Don't allow for user input if auto is enabled for touch based objects (AutoTouch mod)
            if (!userTriggered || Auto)
            {
                if (timeOffset > 0 && Auto)
                    ApplyResult(Result.Judgement.MaxResult);
                return;
            }

            ApplyResult(Result.Judgement.MaxResult);
        }

        private bool checkForTouchInput()
        {
            var touchInput = SentakkiActionInputManager.CurrentState.Touch;

            // Avoiding Linq to minimize allocations, since this would be called every update of this node
            for (TouchSource t = TouchSource.Touch1; t <= TouchSource.Touch10; ++t)
            {
                if (touchInput.GetTouchPosition(t) is Vector2 touchPosition && ReceivePositionalInputAt(touchPosition))
                    return true;
            }

            return false;
        }

        public new void ApplyResult(Action<JudgementResult> application) => base.ApplyResult(application);
    }
}
