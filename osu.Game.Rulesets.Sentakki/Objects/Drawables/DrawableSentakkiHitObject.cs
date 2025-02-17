using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI;

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

        public DrawableSentakkiHitObject()
            : this(null)
        {
        }

        public DrawableSentakkiHitObject(SentakkiHitObject? hitObject = null)
            : base(hitObject!)
        {
        }

        [Resolved]
        protected DrawableSentakkiRuleset? DrawableSentakkiRuleset { get; private set; }

        public Bindable<bool> ExBindable = new Bindable<bool>();

        protected override void OnApply()
        {
            base.OnApply();
            AccentColour.BindTo(HitObject.ColourBindable);
            ExBindable.BindTo(HitObject.ExBindable);
        }

        protected override void OnFree()
        {
            base.OnFree();
            AccentColour.UnbindFrom(HitObject.ColourBindable);
            ExBindable.UnbindFrom(HitObject.ExBindable);
        }

        protected override void Update()
        {
            base.Update();
            UpdateNoteVisuals();
        }

        protected virtual void UpdateNoteVisuals() { }
    }
}
