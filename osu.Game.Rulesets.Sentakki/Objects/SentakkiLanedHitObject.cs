using System.Threading;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public abstract class SentakkiLanedHitObject : SentakkiHitObject
    {
        protected virtual bool NeedBreakSample => true;

        public readonly BindableBool BreakBindable = new BindableBool();

        public bool Break
        {
            get => BreakBindable.Value;
            set => BreakBindable.Value = value;
        }

        public readonly BindableInt LaneBindable = new BindableInt();
        public int Lane
        {
            get => LaneBindable.Value;
            set => LaneBindable.Value = value;
        }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            if (Break)
            {
                for (int i = 0; i < 4; ++i)
                {
                    var nestedObject = new ScorePaddingObject() { StartTime = this.GetEndTime() };

                    if (i == 0 && NeedBreakSample)
                        nestedObject.Samples.Add(new SentakkiHitSampleInfo("Break"));

                    AddNested(nestedObject);
                }
            }
        }
    }
}
