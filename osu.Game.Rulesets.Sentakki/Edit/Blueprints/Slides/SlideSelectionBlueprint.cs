using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides.Visualiser;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
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
        private readonly Container<SlidePathVisualiser> slideVisualisers;

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
                slideVisualisers = new Container<SlidePathVisualiser>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                },
                tapHighlight = new SlideTapHighlight(),

            };

            foreach (var body in hitObject.SlideBodies)
            {
                slideBodyHighlights.Add(new SlideBodyHighlight(body.SlideBodyInfo));
                slideVisualisers.Add(new SlidePathVisualiser(hitObject, body.SlideBodyInfo, hitObject.Lane));
            }
        }

        protected override void OnDeselected()
        {
            foreach (var sb in slideBodyHighlights)
                sb.OnDeselected();

            base.OnDeselected();
        }

        protected override void OnSelected()
        {
            foreach (var sb in slideBodyHighlights)
                sb.OnSelected();

            base.OnSelected();
        }

        protected override void Update()
        {
            base.Update();

            updateTapHighlight();
            updateSlideBodyHighlights();
        }

        [Resolved]
        private SentakkiSnapProvider snapProvider { get; set; } = null!;

        private void updateTapHighlight()
        {
            var slideTap = DrawableObject.SlideTaps.Child;

            tapHighlight.SlideTapPiece.Scale = slideTap.TapVisual.Scale;
            tapHighlight.SlideTapPiece.Stars.Rotation = ((SlideTapPiece)slideTap.TapVisual).Stars.Rotation;
            tapHighlight.SlideTapPiece.SecondStar.Alpha = ((SlideTapPiece)slideTap.TapVisual).SecondStar.Alpha;
            tapHighlight.Rotation = DrawableObject.HitObject.Lane.GetRotationForLane();
            tapHighlight.SlideTapPiece.Y = -snapProvider.GetDistanceRelativeToCurrentTime(DrawableObject.HitObject.StartTime, SentakkiPlayfield.NOTESTARTDISTANCE, SentakkiPlayfield.INTERSECTDISTANCE);
        }

        private void updateSlideBodyHighlights()
        {
            slideVisualisers.Rotation = DrawableObject.HitObject.Lane.GetRotationForLane() - 22.5f;
            slideBodyHighlights.Rotation = slideVisualisers.Rotation = DrawableObject.HitObject.Lane.GetRotationForLane() - 22.5f;

            for (int i = 0; i < DrawableObject.SlideBodies.Count; ++i)
            {
                var slideBody = DrawableObject.SlideBodies[i];

                slideBodyHighlights[i].UpdateFrom(slideBody);
            }
        }

        public override Vector2 ScreenSpaceSelectionPoint => tapHighlight.ScreenSpaceDrawQuad.Centre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => tapHighlight.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => tapHighlight.ScreenSpaceDrawQuad;
    }
}
