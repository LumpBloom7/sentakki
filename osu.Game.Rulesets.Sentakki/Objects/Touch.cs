using osu.Framework.Bindables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Touch : SentakkiHitObject, IBreakNote, IExNote
    {
        public override Color4 DefaultNoteColour => Color4.Aqua;

        public Vector2 Position { get; set; }

        public BindableBool BreakBindable { get; } = new BindableBool();

        public bool Break
        {
            get => BreakBindable.Value;
            set => BreakBindable.Value = value;
        }

        public bool Ex { get; set; }

        protected override HitWindows CreateHitWindows() => new SentakkiTouchHitWindows();
    }
}
