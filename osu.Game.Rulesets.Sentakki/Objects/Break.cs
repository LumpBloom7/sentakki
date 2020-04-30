using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
using osu.Game.Rulesets.Sentakki.Scoring;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Break : Tap
    {
        public override Judgement CreateJudgement() => new SentakkiBreakJudgement();
        protected override HitWindows CreateHitWindows() => new SentakkiHitWindows();
    }
}
