using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public abstract class SentakkiLanedHitObject : SentakkiHitObject
    {
        public readonly BindableInt LaneBindable = new BindableInt(0);
        public virtual int Lane
        {
            get => LaneBindable.Value;
            set => LaneBindable.Value = value;
        }
    }
}
