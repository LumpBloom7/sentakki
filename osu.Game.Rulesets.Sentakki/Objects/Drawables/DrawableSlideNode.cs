using osu.Game.Rulesets.Scoring;
using System.Diagnostics;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using System.Linq;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideNode : DrawableSentakkiHitObject
    {
        public override bool HandlePositionalInput => true;
        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;
        protected DrawableSlide Slide;

        protected int ThisIndex;
        public DrawableSlideNode(Slide.SlideNode node, DrawableSlide slideNote)
            : base(node)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Slide = slideNote;
            RelativeSizeAxes = Axes.None;
            Position = slideNote.Slidepath.Path.PositionAt((HitObject as Slide.SlideNode).Progress);
            Size = new Vector2(160);
            CornerExponent = 2f;
            CornerRadius = 80;
            Masking = true;
            BorderColour = Color4.White;
            BorderThickness = 2;
            AddInternal(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0,
                AlwaysPresent = true
            });
        }
        protected override void LoadComplete()
        {
            base.LoadComplete();
            ThisIndex = Slide.SlideNodes.IndexOf(this);
        }

        protected bool IsHittable => ThisIndex < 2 || Slide.SlideNodes[ThisIndex - 2].IsHit;

        protected void HitPreviousNodes()
        {
            foreach (var node in Slide.SlideNodes)
            {
                if (node == this) return;
                if (!node.Result.HasResult)
                    node.forceJudgement();
            }
        }

        // Needs work :)
        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (!userTriggered) return;
            if (!IsHittable)
                return;

            HitPreviousNodes();
            ApplyResult(r => r.Type = HitResult.Perfect);
            Slide.Slidepath.Progress = (HitObject as Slide.SlideNode).Progress;
        }
        public void UpdateResult() => base.UpdateResult(true);

        // Forces this object to have a result.
        private void forceJudgement() => ApplyResult(r => r.Type = HitResult.Perfect);

        protected override void Update()
        {
            if (IsHovered)
                if (SentakkiActionInputManager.PressedActions.Any())
                    UpdateResult(true);
        }
    }
}
