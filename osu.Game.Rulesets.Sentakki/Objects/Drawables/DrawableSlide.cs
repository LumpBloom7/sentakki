using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osuTK;
using osuTK.Graphics;
using System.Linq;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlide : DrawableSentakkiHitObject
    {
        public override bool DisplayResult => false;

        public Container<DrawableSlideNode> SlideNodes;
        public Container<DrawableSlideTap> SlideTaps;
        public SlideBody Slidepath;

        // Allows us to manage the slide body independently from the Nested Tap drawable, which will handle itself
        private Container slideBodyContainer;
        public StarPiece SlideStar;

        protected override double InitialLifetimeOffset => 8000;

        private float starProg = 0;
        public float StarProgress
        {
            get => starProg;
            set
            {
                starProg = value;
                SlideStar.Position = Slidepath.Path.PositionAt(value);
                if (Auto)
                    Slidepath.Progress = StarProgress;
            }
        }
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
                slideBodyContainer = new Container{
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]{
                        Slidepath = new SlideBody
                        {
                            Alpha = 0,
                            Path = (hitObject as Slide).SlidePath,
                        },
                        new Container{
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Child = SlideStar = new StarPiece
                            {
                                Alpha = 0,
                                Scale = Vector2.Zero,
                                Position = Slidepath.Path.PositionAt(0),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes  = Axes.None,
                                Size = new Vector2(80),
                            }
                        },
                        SlideNodes = new Container<DrawableSlideNode>
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
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
            HitObject.LaneBindable.BindValueChanged(l =>
            {
                // The slide paths always start from lane 0, so we need to compensate for that
                slideBodyContainer.Rotation = l.NewValue.GetRotationForLane() - 22.5f;
                SlideTaps.Child.HitObject.Lane = l.NewValue;
            }, true);

            AccentColour.BindValueChanged(c =>
            {
                slideBodyContainer.Colour = c.NewValue;
            }, true);
            SlideTaps.Child.AccentColour.BindTo(AccentColour);
            SlideTaps.Child.Auto = Auto;
        }
        protected override void UpdateInitialTransforms()
        {
            using (BeginAbsoluteSequence(HitObject.StartTime - 500, true))
            {
                Slidepath.FadeInFromZero(500);
                using (BeginAbsoluteSequence(HitObject.StartTime - 50, true))
                {
                    SlideStar.FadeInFromZero(100).ScaleTo(1, 100);
                    this.Delay(100).TransformTo(nameof(StarProgress), 1f, (HitObject as IHasDuration).Duration - 50);
                }
            }
        }
        protected override void UpdateStateTransforms(ArmedState state) { }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            SlideNodes.Clear();
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Tap x:
                    return new DrawableSlideTap(x)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    };
                case Slide.SlideTailNode tailNode:
                    return new DrawableSlideTailNode(tailNode, this)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    };
                case Slide.SlideNode node:
                    return new DrawableSlideNode(node, this)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    };
            }

            return base.CreateNestedHitObject(hitObject);
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);

            switch (hitObject)
            {
                case DrawableSlideTailNode tailNode:
                    SlideNodes.Add(tailNode);
                    break;
                case DrawableSlideNode node:
                    SlideNodes.Add(node);
                    break;
                case DrawableSlideTap tap:
                    SlideTaps.Child = tap;
                    break;
            }
        }
        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (SlideNodes.Children.Last().AllJudged)
            {
                ApplyResult(r => r.Type = HitResult.Perfect);
            }
        }
    }
}