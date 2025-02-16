using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public partial class DrawableSentakkiHitObject : DrawableHitObject<SentakkiHitObject>
    {
        protected override double InitialLifetimeOffset => AnimationDuration.Value;

        public readonly BindableBool AutoBindable = new BindableBool();

        public bool Auto
        {
            get => AutoBindable.Value;
            set => AutoBindable.Value = value;
        }

        // Used for the animation update
        protected readonly Bindable<double> AnimationDuration = new Bindable<double>(1000);

        protected override float SamplePlaybackPosition => (Position.X / (SentakkiPlayfield.INTERSECTDISTANCE * 2)) + 0.5f;

        private Container<DrawableScorePaddingObject> scorePaddingObjects = null!;
        private PausableSkinnableSound breakSample = null!;

        public DrawableSentakkiHitObject()
            : this(null)
        {
        }

        public DrawableSentakkiHitObject(SentakkiHitObject? hitObject = null)
            : base(hitObject!)
        {
            AddRangeInternal([
                scorePaddingObjects = new Container<DrawableScorePaddingObject>(),
                breakSample = new PausableSkinnableSound()
            ]);
        }

        protected override void LoadSamples()
        {
            base.LoadSamples();

            LoadBreakSamples();
        }

        public void LoadBreakSamples()
        {
            breakSample.Samples = HitObject.CreateBreakSample();
        }

        public override void PlaySamples()
        {
            base.PlaySamples();

            breakSample.Balance.Value = CalculateSamplePlaybackBalance(SamplePlaybackPosition);
            breakSample.Play();
        }

        [Resolved]
        protected DrawableSentakkiRuleset? DrawableSentakkiRuleset { get; private set; }

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();
            AnimationDuration.BindValueChanged(_ => queueTransformReset(), true);
        }

        public Bindable<bool> ExBindable = new Bindable<bool>();

        protected override void OnApply()
        {
            base.OnApply();
            AccentColour.BindTo(HitObject.ColourBindable);
            ExBindable.BindTo(HitObject.ExBindable);
        }

        private bool transformResetQueued;

        protected override void Update()
        {
            base.Update();

            if (transformResetQueued) RefreshStateTransforms();
        }

        // We need to make sure the current transform resets, perhaps due to animation duration being changed
        // We don't want to reset the transform of all DHOs immediately,
        // since repeatedly resetting transforms of non-present DHO is wasteful
        private void queueTransformReset()
        {
            transformResetQueued = true;
            //LifetimeStart = HitObject.StartTime - InitialLifetimeOffset;
        }

        protected override void UpdateInitialTransforms()
        {
            // The transform is reset as soon as this function begins
            // This includes the usual LoadComplete() call, or rewind resets
            transformResetQueued = false;
        }

        protected new void ApplyResult(HitResult hitResult)
        {
            // Also give Break note score padding a judgement
            for (int i = 0; i < scorePaddingObjects.Count; ++i)
                scorePaddingObjects[^(i + 1)].ApplyResult(hitResult);

            base.ApplyResult(hitResult);
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case ScorePaddingObject p:
                    return new DrawableScorePaddingObject(p);


                default:
                    return base.CreateNestedHitObject(hitObject);

            }
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableScorePaddingObject p:
                    scorePaddingObjects.Add(p);
                    break;


                default:
                    base.AddNestedHitObject(hitObject);
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            scorePaddingObjects.Clear(false);
        }

        protected override void OnFree()
        {
            base.OnFree();
            AccentColour.UnbindFrom(HitObject.ColourBindable);
            ExBindable.UnbindFrom(HitObject.ExBindable);
            breakSample.ClearSamples();
        }

    }
}
