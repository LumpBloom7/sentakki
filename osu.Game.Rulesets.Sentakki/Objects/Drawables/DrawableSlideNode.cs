using osu.Game.Rulesets.Scoring;
using System.Diagnostics;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideNode : DrawableSentakkiHitObject
    {
        protected DrawableSlide Slide;
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

        // Needs work :)
        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            int currentIndex = Slide.SlideNodes.IndexOf(this);
            if (!userTriggered) return;
            if (currentIndex >= 2 && !Slide.SlideNodes[currentIndex - 2].IsHit)
                return;

            if (!Slide.SlideNodes[currentIndex - 1].Result.HasResult)
                Slide.SlideNodes[currentIndex - 1].ForceJudgement();

            ApplyResult(r => r.Type = HitResult.Perfect);


            Slide.Slidepath.Progress = (HitObject as Slide.SlideNode).Progress;
        }
        public void UpdateResult() => base.UpdateResult(true);

        // Forces this object to have a result.
        public void ForceJudgement() => ApplyResult(r => r.Type = HitResult.Perfect);
    }
}
