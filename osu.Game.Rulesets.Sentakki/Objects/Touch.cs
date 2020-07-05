using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Touch : SentakkiHitObject
    {
        public override bool IsBreak => false;
        public override Color4 NoteColor => HasTwin ? Color4.Gold : Color4.Cyan;

        public override float Angle => 0;

        protected override HitWindows CreateHitWindows() => new SentakkiTouchHitWindows();
    }
}
