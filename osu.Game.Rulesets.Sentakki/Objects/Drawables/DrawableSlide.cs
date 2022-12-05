using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public partial class DrawableSlide : DrawableSentakkiHitObject
    {
        public new Slide HitObject => (Slide)base.HitObject;

        public override bool DisplayResult => false;

        public Container<DrawableSlideBody> SlideBodies = null!;
        public Container<DrawableSlideTap> SlideTaps = null!;

        public DrawableSlide()
            : this(null)
        {
        }

        public DrawableSlide(SentakkiHitObject? hitObject = null)
            : base(hitObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = Vector2.Zero;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            AddRangeInternal(new Drawable[]
            {
                SlideBodies = new Container<DrawableSlideBody>
                {
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

        // This shouldn't play any samples
        protected override void LoadSamples()
        {
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            // We also make sure all transforms have finished to avoid jank
            for (int i = 0; i < NestedHitObjects.Count; i++)
            {
                var nested = NestedHitObjects[i];
                if (!nested.Result.HasResult || Time.Current < nested.LatestTransformEndTime)
                    return;
            }

            ApplyResult(Result.Judgement.MaxResult);
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case SlideTap x:
                    return new DrawableSlideTap(x)
                    {
                        AutoBindable = { BindTarget = AutoBindable },
                    };

                case SlideBody slideBody:
                    return new DrawableSlideBody(slideBody)
                    {
                        AutoBindable = { BindTarget = AutoBindable },
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    };
            }

            return base.CreateNestedHitObject(hitObject);
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableSlideBody body:
                    SlideBodies.Add(body);
                    break;

                case DrawableSlideTap tap:
                    SlideTaps.Child = tap;
                    break;
            }

            base.AddNestedHitObject(hitObject);
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            SlideBodies.Clear(false);
            SlideTaps.Clear(false);
        }
    }
}
