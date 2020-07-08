using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Touch : SentakkiHitObject
    {
        // Currently to avoid conflict issues
        //public new Vector2 Position { get; set; }

        public override bool IsBreak => false;
        public override Color4 NoteColor => HasTwin ? Color4.Gold : Color4.Cyan;
        protected override HitWindows CreateHitWindows() => new SentakkiTouchHitWindows();
    }
}
