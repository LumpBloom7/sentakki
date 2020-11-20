using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Configuration;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSentakkiJudgement : DrawableJudgement
    {
        public DrawableSentakkiJudgement(JudgementResult result, DrawableSentakkiHitObject judgedObject)
            : base(result, judgedObject)
        {
        }

        protected override void ApplyHitAnimations()
        {
            JudgementBody.ScaleTo(0.9f);
            JudgementBody.ScaleTo(1, 50, Easing.OutElastic);

            JudgementBody.Delay(50)
                         .ScaleTo(0.8f, 300)
                         .FadeOut(250);
        }
    }
}
