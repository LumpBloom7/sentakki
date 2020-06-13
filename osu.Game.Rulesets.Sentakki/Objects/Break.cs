using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
using osu.Game.Rulesets.Sentakki.Scoring;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Break : Tap
    {
        public override Color4 NoteColor { get; set; } = Color4.OrangeRed;
        public override Judgement CreateJudgement() => new SentakkiBreakJudgement();
        protected override HitWindows CreateHitWindows() => new SentakkiHitWindows();
    }
}
