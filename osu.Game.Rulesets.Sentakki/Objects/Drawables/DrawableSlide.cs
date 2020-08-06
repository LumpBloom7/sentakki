using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Beatmaps.ControlPoints;
using osuTK;
using osuTK.Graphics;
using System.Linq;
using osu.Game.Beatmaps;



namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlide : DrawableSentakkiHitObject
    {
        public override bool DisplayResult => false;

        protected override bool PlayBreakSample => false;

        public Container<DrawableSlideNode> SlideNodes;
        public Container<DrawableSlideTap> SlideTaps;
        public SlideBody Slidepath;

        // Allows us to manage the slide body independently from the Nested Tap drawable, which will handle itself
        private Container slideBodyContainer;
        public StarPiece SlideStar;

        protected override double InitialLifetimeOffset => 500;

        private float starProg = 0;
        private Vector2? previousPosition = null;
        public float StarProgress
        {
            get => starProg;
            set
            {
                starProg = value;
                SlideStar.Position = Slidepath.Path.PositionAt(value);
                if (previousPosition == null)
                    SlideStar.Rotation = SlideStar.Position.GetDegreesFromPosition(Slidepath.Path.PositionAt(value + .001f));
                else
                    SlideStar.Rotation = previousPosition.Value.GetDegreesFromPosition(SlideStar.Position);
                previousPosition = SlideStar.Position;
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
                            Path = (hitObject as Slide).SlidePath.Path,
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
        }

        [Resolved]
        private Bindable<WorkingBeatmap> workingBeatmap { get; set; }

        public double ShootDelay => workingBeatmap.Value.Beatmap.ControlPointInfo.TimingPointAt(HitObject.StartTime).BeatLength;

        protected override void UpdateInitialTransforms()
        {
            using (BeginAbsoluteSequence(HitObject.StartTime - 500, true))
            {
                Slidepath.FadeIn(500);
                using (BeginAbsoluteSequence(HitObject.StartTime - 50, true))
                {
                    //???
                    SlideStar.FadeIn(100).ScaleTo(1, 100);
                    this.Delay(100 + ShootDelay).TransformTo(nameof(StarProgress), 1f, (HitObject as IHasDuration).Duration - 50 - ShootDelay);
                }
            }
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);
            const double time_fade_miss = 400 /* time_fade_miss = 400 */;
            switch (state)
            {
                case ArmedState.Hit:
                    this.FadeOut();
                    break;
                case ArmedState.Miss:
                    using (BeginDelayedSequence((HitObject as IHasDuration).Duration + SlideNodes.Last().Result.TimeOffset, true))
                    {
                        slideBodyContainer.FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint).FadeOut(time_fade_miss);
                        this.Delay(time_fade_miss).Expire();
                    }
                    break;
            }
        }

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
                    return new DrawableSlideTap(x, this)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AccentColour = { BindTarget = AccentColour }
                    };
                case Slide.SlideNode node:
                    return new DrawableSlideNode(node, this)
                    {
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
                case DrawableSlideNode node:
                    SlideNodes.Add(node);
                    break;
                case DrawableSlideTap tap:
                    SlideTaps.Child = tap;
                    break;
            }
            base.AddNestedHitObject(hitObject);
        }
        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (SlideNodes.Children.Last().AllJudged)
            {
                ApplyResult(r => r.Type = SlideNodes.Children.Last().Result.IsHit ? HitResult.Perfect : HitResult.Miss);
            }
        }
    }
}