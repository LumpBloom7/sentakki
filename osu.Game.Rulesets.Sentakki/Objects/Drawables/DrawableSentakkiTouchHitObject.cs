using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSentakkiTouchHitObject : DrawableSentakkiHitObject
    {
        public BindableBool AutoTouchBindable = new BindableBool();

        public bool AutoTouch
        {
            get => AutoTouchBindable.Value;
            set => AutoTouchBindable.Value = value;
        }

        public DrawableSentakkiTouchHitObject(SentakkiHitObject hitObject)
            : base(hitObject) { }

        protected override void InvalidateTransforms() { }
    }
}
