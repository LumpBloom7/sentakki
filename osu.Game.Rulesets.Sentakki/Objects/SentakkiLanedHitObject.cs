using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public abstract class SentakkiLanedHitObject : SentakkiHitObject, IBreakNote, IExNote
    {
        public readonly BindableInt LaneBindable = new BindableInt();

        public int Lane
        {
            get => LaneBindable.Value;
            set => LaneBindable.Value = value;
        }

        public virtual int BreakScoreWeighting => 5;

        public BindableBool BreakBindable { get; } = new BindableBool();

        public bool Break
        {
            get => BreakBindable.Value;
            set => BreakBindable.Value = value;
        }

        public bool Ex { get; set; }

        protected virtual bool NeedBreakSample => true;

        bool IBreakNote.NeedBreakSample => NeedBreakSample;
    }
}
