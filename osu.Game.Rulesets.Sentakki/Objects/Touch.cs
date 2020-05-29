using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Touch : SentakkiHitObject
    {
        public override float Angle => 0;

        protected override HitWindows CreateHitWindows() => new SentakkiHitWindows();
    }
}
