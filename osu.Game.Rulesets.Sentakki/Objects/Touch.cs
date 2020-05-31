using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Touch : SentakkiHitObject
    {
        public override float Angle => 0;

        // This is not actually used during the result check, since all valid hits result in a perfect judgement
        // The only reason that it's here is so that hits show on the accuracy meter at the side.
        protected override HitWindows CreateHitWindows() => new SentakkiHitWindows();
    }
}
