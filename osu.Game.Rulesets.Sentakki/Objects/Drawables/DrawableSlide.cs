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
        public Container<DrawableTap> SlideTaps = null!;

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
                SlideTaps = new Container<DrawableTap>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });
        }

        protected override void LoadSamples()
        {
            // The slide parent object doesn't need a sample
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            bool allClear = true;

            for (int i = 0; i < NestedHitObjects.Count; i++)
            {
                var nested = NestedHitObjects[i];
                if (!nested.Result.HasResult)
                    return;

                allClear = allClear && nested.Result.IsHit;
            }

            // In order to ensure all animations do not get interrupted, we update the hit state transforms to accommodate the worst result
            ApplyResult(allClear ? Result.Judgement.MaxResult : Result.Judgement.MinResult);
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

                case Tap y:
                    return new DrawableTap(y)
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

                case DrawableSlideTap slideTap:
                    SlideTaps.Child = slideTap;
                    break;

                case DrawableTap tap:
                    SlideTaps.Child = tap;
                    break;
            }

            base.AddNestedHitObject(hitObject);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);
            switch (state)
            {
                case ArmedState.Hit:
                    this.Delay(200).FadeOut().Expire();
                    break;
                case ArmedState.Miss:
                    this.Delay(400).FadeOut().Expire();
                    break;
            }
            Expire();
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            SlideBodies.Clear(false);
            SlideTaps.Clear(false);
        }
    }
}
