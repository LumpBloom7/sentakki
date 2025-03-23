using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Touch : SentakkiHitObject, IHasPosition
    {
        public override Color4 DefaultNoteColour => Color4.Aqua;

        private HitObjectProperty<Vector2> position;

        public Bindable<Vector2> PositionBindable => position.Bindable;

        public Vector2 Position
        {
            get => position.Value;
            set => position.Value = value;
        }

        public float X
        {
            get => Position.X;
            set => Position = new(value, Position.Y);
        }

        public float Y
        {
            get => Position.Y;
            set => Position = new Vector2(Position.X, value);
        }

        protected override HitWindows CreateHitWindows() => new SentakkiTouchHitWindows();
    }
}
