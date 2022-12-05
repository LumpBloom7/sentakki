using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides
{
    public partial class SlideSelectionBlueprint : SentakkiSelectionBlueprint<Slide>
    {
        public new DrawableSlide DrawableObject => (DrawableSlide)base.DrawableObject;

        private readonly SlideTapHighlight tapHighlight;

        private readonly Container<SlideBodyHighlight> slideBodyHighlights;

        public SlideSelectionBlueprint(Slide hitObject)
            : base(hitObject)
        {
            InternalChildren = new Drawable[]
            {
                slideBodyHighlights = new Container<SlideBodyHighlight>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Rotation = -22.5f,
                },
                tapHighlight = new SlideTapHighlight(),
            };

            foreach (var body in hitObject.SlideBodies)
                slideBodyHighlights.Add(new SlideBodyHighlight(body.SlideBodyInfo));
        }

        protected override void Update()
        {
            base.Update();

            updateTapHighlight();
            updateSlideBodyHighlights();
        }

        private void updateTapHighlight()
        {
            var slideTap = DrawableObject.SlideTaps.Child;

            tapHighlight.SlideTapPiece.Scale = slideTap.TapVisual.Scale;
            tapHighlight.SlideTapPiece.Stars.Rotation = ((SlideTapPiece)slideTap.TapVisual).Stars.Rotation;
            tapHighlight.SlideTapPiece.SecondStar.Alpha = ((SlideTapPiece)slideTap.TapVisual).SecondStar.Alpha;
            tapHighlight.Rotation = DrawableObject.HitObject.Lane.GetRotationForLane();
            tapHighlight.SlideTapPiece.Y = Math.Max(slideTap.TapVisual.Y, -SentakkiPlayfield.INTERSECTDISTANCE);
        }

        private void updateSlideBodyHighlights()
        {
            for (int i = 0; i < DrawableObject.SlideBodies.Count; ++i)
            {
                var slideBody = DrawableObject.SlideBodies[i];

                slideBodyHighlights[i].UpdateFrom(slideBody);
                slideBodyHighlights[i].Rotation = DrawableObject.HitObject.Lane.GetRotationForLane();
            }
        }

        public override Vector2 ScreenSpaceSelectionPoint => tapHighlight.ScreenSpaceDrawQuad.Centre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => tapHighlight.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => tapHighlight.ScreenSpaceDrawQuad;
    }
}
