using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public partial class DrawableSlideCheckpointNode : DrawableSentakkiHitObject
    {
        public new SlideCheckpoint.CheckpointNode HitObject => (SlideCheckpoint.CheckpointNode)base.HitObject;

        private DrawableSlideCheckpoint checkpoint => (DrawableSlideCheckpoint)ParentHitObject;

        public override bool HandlePositionalInput => true;
        public override bool DisplayResult => false;

        private SentakkiInputManager sentakkiActionInputManager = null!;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= ((SentakkiInputManager)GetContainingInputManager());

        public const float DETECTION_RADIUS = 100;

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
            Size = new Vector2(DETECTION_RADIUS * 2);
            CornerExponent = 2f;
            CornerRadius = DETECTION_RADIUS;
        }

        protected override void OnApply()
        {
            base.OnApply();
            Position = HitObject.Position;
        }

        private int pressedCount = 0;

        protected override void Update()
        {
            base.Update();

            int updatedPressedCounts = countActiveTouchPoints();

            if (updatedPressedCounts > pressedCount)
                UpdateResult(true);

            pressedCount = updatedPressedCounts;
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

            if (!checkpoint.IsHittable)
                return;

            ApplyResult(Result.Judgement.MaxResult);
        }

        private int countActiveTouchPoints()
        {
            var touchInput = SentakkiActionInputManager.CurrentState.Touch;
            int count = 0;

            bool isPressing = false;
            foreach (var item in SentakkiActionInputManager.PressedActions)
            {
                if (item < SentakkiAction.Key1)
                {
                    isPressing = true;
                    break;
                }
            }

            if (isPressing && ReceivePositionalInputAt(SentakkiActionInputManager.CurrentState.Mouse.Position))
                ++count;

            foreach (TouchSource source in touchInput.ActiveSources)
            {
                if (touchInput.GetTouchPosition(source) is Vector2 touchPosition && ReceivePositionalInputAt(touchPosition))
                    ++count;
            }

            return count;
        }

        public new void ApplyResult(HitResult result)
        {
            if (Judged)
                return;

            base.ApplyResult(result);
        }
    }
}
