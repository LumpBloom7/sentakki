using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
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
            AnimationDuration.BindValueChanged(_ =>
            {
                // We need to make sure the current transform is refreshed if the animation duration changes
                // However, we don't want to refresh the transforms of hitobjects that isn't actively in use
                if (IsInUse)
                    RefreshStateTransforms();
            }, true);
        }

        public Bindable<bool> ExBindable = new Bindable<bool>();

        protected override void OnApply()
        {
            base.OnApply();
            AccentColour.BindTo(HitObject.ColourBindable);
            ExBindable.BindTo(HitObject.ExBindable);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);

            if (state is not ArmedState.Idle)
                return;

            // This is a very aggressive lifetime optimisation to ensure that lifetimes are sane even in editor contexts (or any non-frame stable contexts)
            // The beginning of DHO.UpdateState() sets LifetimeEnd to double.MaxValue, regardless of the LifetimeEnd set in the LifetimeEntry
            // After UpdateHitStateTransforms(), the LifetimeEnd only gets set iff the current ArmedState is not Idle.
            // In non-frame-stable contexts, the DHO may be skipped over, so it is never hit/missed and therefore the ArmedState is Idle despite CurrentTime being past the miss window.
            // This manual expiry aims to mitigate the problem.
            // Possibly similar case: https://github.com/ppy/osu/blob/143593b3b90977d0a91da833eccc3efd39c39254/osu.Game.Rulesets.Osu/Objects/Drawables/DrawableHitCircle.cs#L208
            this.Delay(HitObject.MaximumJudgementOffset + 50).FadeOut();
        }

        protected new void ApplyResult(HitResult hitResult)
        {
            // Apply Ex results if necessary
            if (hitResult < HitResult.Perfect && HitObject.Ex && hitResult.IsHit())
                hitResult = HitResult.Great;

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
