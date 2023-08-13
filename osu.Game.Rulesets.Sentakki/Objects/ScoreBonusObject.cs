using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class ScoreBonusObject : HitObject
    {
        public override Judgement CreateJudgement() => new SentakkiBonusJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
