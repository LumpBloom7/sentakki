using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Configuration;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    // Cached so that SlideTapPiece can access via DI, and adjust visuals to account for multiple slide bodies
    [Cached]
    public class DrawableSlide : DrawableSentakkiHitObject
    {
        public override bool DisplayResult => false;

        public Container<DrawableSlideBody> SlideBodies;
        public Container<DrawableSlideTap> SlideTaps;

        public DrawableSlide(SentakkiHitObject hitObject)
            : base(hitObject)
        {
            AccentColour.BindTo(HitObject.ColourBindable);
            Size = Vector2.Zero;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
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

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfig)
        {
            // This is to ensure the container is alive when the child is.
            sentakkiConfig?.BindWith(SentakkiRulesetSettings.AnimationDuration, AnimationDuration);
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
                        AutoBindable = { BindTarget = AutoBindable },
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AccentColour = { BindTarget = AccentColour }
                    };
                case SlideBody slideBody:
                    return new DrawableSlideBody(slideBody)
                    {
                        AutoBindable = { BindTarget = AutoBindable },
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
