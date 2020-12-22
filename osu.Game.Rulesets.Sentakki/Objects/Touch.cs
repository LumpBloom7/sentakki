using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Touch : SentakkiHitObject
    {
        protected override Color4 DefaultNoteColour => Color4.Aqua;

        public Vector2 Position { get; set; }

        protected override HitWindows CreateHitWindows() => new SentakkiTouchHitWindows();
    }
}
