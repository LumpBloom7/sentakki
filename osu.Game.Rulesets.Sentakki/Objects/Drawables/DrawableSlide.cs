using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects;
using osuTK;
using osuTK.Graphics;
using System.Linq;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    // Cached so that SlideTapPiece can access via DI, and adjust visuals to account for multiple slide bodies
    [Cached]
    public class DrawableSlide : DrawableSentakkiTouchHitObject
    {
        public override bool DisplayResult => false;

        protected override bool PlayBreakSample => false;

        public Container<DrawableSlideBody> SlideBodies;
        public Container<DrawableSlideTap> SlideTaps;

        protected override double InitialLifetimeOffset => 1000;

        public DrawableSlide(SentakkiHitObject hitObject)
            : base(hitObject)
        {
            AccentColour.Value = hitObject.NoteColor;
            Size = Vector2.Zero;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            AlwaysPresent = true;
            AddRangeInternal(new Drawable[]
            {
                SlideBodies = new Container<DrawableSlideBody>{
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                SlideTaps = new Container<DrawableSlideTap>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            SlideBodies.Clear();
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Tap x:
                    return new DrawableSlideTap(x, this)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AccentColour = { BindTarget = AccentColour }
                    };
                case SlideBody slideBody:
                    return new DrawableSlideBody(slideBody)
                    {
                        AutoTouchBindable = { BindTarget = AutoTouchBindable },
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AccentColour = { BindTarget = AccentColour }
                    };
            }

            return base.CreateNestedHitObject(hitObject);
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableSlideBody node:
                    SlideBodies.Add(node);
                    break;
                case DrawableSlideTap tap:
                    SlideTaps.Child = tap;
                    break;
            }
            base.AddNestedHitObject(hitObject);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (NestedHitObjects.All(n => n.Result.HasResult && Time.Current >= n.LatestTransformEndTime))
                ApplyResult(r => r.Type = r.Judgement.MaxResult);
        }
    }
}
