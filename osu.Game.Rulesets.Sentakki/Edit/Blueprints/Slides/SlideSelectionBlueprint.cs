using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.PolygonExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Events;
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
                tapHighlight = new SlideTapHighlight(),
                slideVisualisers = new Container<SlidePathVisualiser>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                },
            };

            recreateVisualisations();
        }

        protected override void OnDeselected()
        {
            foreach (var sb in slideBodyHighlights)
                sb.OnDeselected();

            foreach (var sv in slideVisualisers)
                sv.Deselect();

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

        private void recreateVisualisations()
        {
            slideBodyHighlights.Clear();
            slideVisualisers.Clear();
            foreach (var si in HitObject.SlideInfoList)
            {
                slideBodyHighlights.Add(new SlideBodyHighlight(si));
                slideVisualisers.Add(new SlidePathVisualiser(HitObject, si, HitObject.Lane));
            }

            if (IsSelected && HitObject.SlideInfoList.Count > 0)
            {
                slideVisualisers[0].Select();
            }
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
            tapHighlight.SlideTapPiece.Y = -snapProvider.GetDistanceRelativeToCurrentTime(DrawableObject.HitObject.StartTime, SentakkiPlayfield.NOTESTARTDISTANCE);
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

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (!IsSelected)
                return false;

            if (tapHighlight.ReceivePositionalInputAt(e.ScreenSpaceMousePosition))
            {
                foreach (var sv in slideVisualisers)
                    sv.Deselect();

                return false;
            }

            var currentSelection = slideVisualisers.SingleOrDefault(sv => sv.Alpha > 0);
            var candidates = slideVisualisers.Where(sv => sv.ReceivePositionalInputAt(e.ScreenSpaceMousePosition)).ToList();

            if (candidates.Count == 0)
                return false;

            if (e.Button != osuTK.Input.MouseButton.Left)
                return false;

            currentSelection?.Deselect();
            int skipAmount = candidates.IndexOf(currentSelection!) + 1;

            bool tryDHOCycle = false;
            if (skipAmount >= candidates.Count)
            {
                skipAmount = 0;
                tryDHOCycle = true;
            }

            candidates.Skip(skipAmount).First().Select();
            return !tryDHOCycle;
        }

        public override Vector2 ScreenSpaceSelectionPoint => tapHighlight.ScreenSpaceDrawQuad.Centre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => tapHighlight.ReceivePositionalInputAt(screenSpacePos) || slideVisualisers.Any(v => v.ReceivePositionalInputAt(screenSpacePos));

        public override Quad SelectionQuad
        {
            get
            {
                RectangleF selectionRect = tapHighlight.ScreenSpaceDrawQuad.AABBFloat;

                foreach (var sv in slideBodyHighlights)
                {
                    var rect = sv.ScreenSpaceDrawQuad.AABBFloat;

                    selectionRect = RectangleF.Union(selectionRect, rect);
                }

                return Quad.FromRectangle(selectionRect);
            }
        }
    }
}
