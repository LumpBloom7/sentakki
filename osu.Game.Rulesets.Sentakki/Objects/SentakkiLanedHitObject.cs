using osu.Framework.Bindables;
using osu.Game.Rulesets.Sentakki;
using osu.Game.Rulesets.Objects;
using System.Threading;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public abstract class SentakkiLanedHitObject : SentakkiHitObject
    {
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
                for (int i = 0; i < 4; ++i)
                    AddNested(new ScorePaddingObject() { StartTime = this.GetEndTime() });
        }

        public override Color4 NoteColor => Break ? Color4.OrangeRed : base.NoteColor;
    }
}
