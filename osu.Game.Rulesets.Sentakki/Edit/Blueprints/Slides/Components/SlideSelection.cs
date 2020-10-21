using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;
using System.Linq;
using osu.Game.Screens.Edit;
using System;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides.Components
{
    public class SlideSelection : BlueprintPiece<Slide>
    {
        // This needs to be shrunk because the AABB box has larger margins for some reason
        public Quad SelectionBoundaries => starPiece.ScreenSpaceDrawQuad.AABBFloat;

        private Drawable starPiece;

        public SlideSelection()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(75);
            AddInternal(starPiece = new StarPiece());
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
        }

        [Resolved]
        private EditorClock editorClock { get; set; }

        public override void UpdateFrom(Slide hitObject)
        {
            base.UpdateFrom(hitObject);
            Rotation = hitObject.Lane.GetRotationForLane();
        }

        public void UpdateFrom(DrawableSlide drawableSlide)
        {
            var lho = drawableSlide.HitObject as SentakkiLanedHitObject;
            Rotation = lho.Lane.GetRotationForLane();
            if (editorClock.CurrentTimeAccurate < drawableSlide.HitObject.StartTime)
                starPiece.Rotation = ((SlideTapPiece)drawableSlide.SlideTaps.Child.TapVisual).Stars.Rotation;
            else
                starPiece.Rotation = 0;
            starPiece.Position = new Vector2(0, Math.Max(drawableSlide.SlideTaps.Child.TapVisual.Position.Y, -296.5f));
        }
    }
}
