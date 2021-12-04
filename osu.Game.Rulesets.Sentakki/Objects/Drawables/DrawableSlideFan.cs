using osu.Framework.Allocation;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideFan : DrawableSlideBody
    {
        public new SlideFan HitObject => (SlideFan)base.HitObject;

        private static Vector2 star_origin_pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0);

        private static Vector2[] star_dest_pos = {
            SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE,3),
            SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE,4),
            SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE,5),
        };

        public override float StarProgress
        {
            get => StarProg;
            set
            {
                StarProg = value;
                for (int i = 0; i < 3; ++i)
                    SlideStars[i].Position = Vector2.Lerp(star_origin_pos, star_dest_pos[i], value);
            }
        }

        public DrawableSlideFan() : this(null) { }
        public DrawableSlideFan(SlideFan hitObject)
            : base(hitObject) { }

        protected override ISlideVisual CreateSlideVisuals() => new SlideFanVisual();

        protected override void CreateSlideStars()
        {
            for (int i = 0; i < 3; ++i)
                SlideStars.Add(new StarPiece
                {
                    Y = -SentakkiPlayfield.INTERSECTDISTANCE,
                    Alpha = 0,
                    Scale = Vector2.Zero,
                    // Each node is 22.5 degrees apart, when measured from a node's perspective.
                    Rotation = 157.5f + (22.5f * i),
                });
        }

        // We don't need to update the path, SlideFan always uses the same visuals
        protected override void UpdateSlidePath() { }
    }
}
