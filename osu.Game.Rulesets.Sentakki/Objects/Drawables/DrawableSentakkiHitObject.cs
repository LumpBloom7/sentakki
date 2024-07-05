using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
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

        protected override float SamplePlaybackPosition => Position.X / (SentakkiPlayfield.INTERSECTDISTANCE * 2);

        private PausableSkinnableSound breakSample = null!;

        private Container<DrawableScorePaddingObject> scorePaddingObjects = null!;

        private Container<DrawableScoreBonusObject> scoreBonusObjects = null!;

        public DrawableSentakkiHitObject()
            : this(null)
        {
        }

        public DrawableSentakkiHitObject(SentakkiHitObject? hitObject = null)
            : base(hitObject!)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                scorePaddingObjects = new Container<DrawableScorePaddingObject>(),
                scoreBonusObjects = new Container<DrawableScoreBonusObject>(),
                breakSample = new PausableSkinnableSound(),
            });
        }

        protected override void LoadSamples()
        {
            base.LoadSamples();

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
            ExBindable.Value = (HitObject as IExNote)?.Ex ?? false;
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case ScorePaddingObject p:
                    return new DrawableScorePaddingObject(p);

                case ScoreBonusObject b:
                    return new DrawableScoreBonusObject(b);
            }

            return base.CreateNestedHitObject(hitObject);
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableScorePaddingObject p:
                    scorePaddingObjects.Add(p);
                    break;

                case DrawableScoreBonusObject b:
                    scoreBonusObjects.Add(b);
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
            scoreBonusObjects.Clear(false);
        }

        protected override JudgementResult CreateResult(Judgement judgement) => new SentakkiJudgementResult(HitObject, judgement);

        protected new void ApplyResult(HitResult result)
        {
            var SentakkiJudgementResult = (SentakkiJudgementResult)Result;
            if (result == HitResult.Perfect)
            {
                SentakkiJudgementResult.Critical = true;
                result = Result.Judgement.MaxResult;
            }
            else
            {
                SentakkiJudgementResult.Critical = false;
            }

            // Judge the scoreBonus
            foreach (var bonusObject in scoreBonusObjects)
                bonusObject.TriggerResult();

            // Also give Break note score padding a judgement
            for (int i = 0; i < scorePaddingObjects.Count; ++i)
                scorePaddingObjects[^(i + 1)].ApplyResult(result);

            base.ApplyResult(result);
        }

        protected override void OnFree()
        {
            base.OnFree();
            AccentColour.UnbindFrom(HitObject.ColourBindable);
            breakSample.ClearSamples();
        }

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

        private bool transformResetQueued;
    }
}
