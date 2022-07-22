using System;
using System.Linq;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides
{
    public class SlideSelectionBlueprint : SentakkiSelectionBlueprint<Slide>
    {
        public new DrawableSlide DrawableObject => (DrawableSlide)base.DrawableObject;

        private readonly List<SlideStarHighlight> starHighlights = new List<SlideStarHighlight>();
        private readonly List<SlideBodyHighlight> bodyHighlights = new List<SlideBodyHighlight>();

        public SlideSelectionBlueprint(Slide hitObject)
            : base(hitObject)
        {
            foreach (var body in hitObject.SlideInfoList)
            {
                switch (body.ID)
                {
                    case 34: // FanID
                        for (int i = 0; i < 3; ++i)
                            starHighlights.Add(new SlideStarHighlight());
                        break;
                    default:
                        starHighlights.Add(new SlideStarHighlight());
                        bodyHighlights.Add(new SlideBodyHighlight());
                        break;
                }
            }

            AddRangeInternal(starHighlights);
            AddRangeInternal(bodyHighlights);
        }

        protected override void Update()
        {
            base.Update();

            var SlideTap = DrawableObject.SlideTaps.Child;

            var FirstSlideBody = DrawableObject.SlideBodies.Any() ? DrawableObject.SlideBodies[0] : null;

            if (DrawableObject.Time.Current < DrawableObject.HitObject.StartTime)
            {
                foreach (var starHighlight in starHighlights)
                {
                    starHighlight.Rotation = SlideTap.HitObject.Lane.GetRotationForLane();
                    starHighlight.Note.Position = new Vector2(0, Math.Max(SlideTap.TapVisual.Y, -SentakkiPlayfield.INTERSECTDISTANCE));
                    starHighlight.Note.Scale = SlideTap.TapVisual.Scale;
                    starHighlight.Note.Rotation = (SlideTap.TapVisual as SlideTapPiece).Stars.Rotation;
                }
            }
            else
            {
                int starIndex = 0;
                foreach (var slideBody in DrawableObject.SlideBodies)
                {
                    foreach (var slideStar in slideBody.SlideStars)
                    {
                        starHighlights[starIndex].Rotation = SlideTap.HitObject.Lane.GetRotationForLane() - 22.5f;
                        starHighlights[starIndex].Note.Position = slideStar.Position;
                        starHighlights[starIndex].Note.Scale = slideStar.Scale;
                        starHighlights[starIndex].Note.Rotation = slideStar.Rotation;
                        ++starIndex;
                    }
                }
            }

            for (int i = 0; i < DrawableObject.SlideBodies.Count; ++i)
            {
                bodyHighlights[i].Rotation = SlideTap.HitObject.Lane.GetRotationForLane() - 22.5f;
                bodyHighlights[i].Path.Vertices = DrawableObject.SlideBodies[i].HitObject.SlideInfo.SlidePath.Vertices;
            }
        }

        public override Vector2 ScreenSpaceSelectionPoint => SelectionQuad.Centre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            => starHighlights.Any(s => s.ReceivePositionalInputAt(screenSpacePos)) || bodyHighlights.Any(b => b.ReceivePositionalInputAt(screenSpacePos));

        public override Quad SelectionQuad
        {
            get
            {
                RectangleF rect = starHighlights[0].ScreenSpaceDrawQuad.AABBFloat;
                for (int i = 1; i < starHighlights.Count; ++i)
                    rect = RectangleF.Union(rect, starHighlights[i].ScreenSpaceDrawQuad.AABBFloat);
                return rect;
            }
        }
    }
}
