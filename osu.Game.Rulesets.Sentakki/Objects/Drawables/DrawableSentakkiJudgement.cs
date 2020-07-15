using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSentakkiJudgement : DrawableJudgement
    {
        [Resolved]
        private OsuColour colours { get; set; }

        private OsuSpriteText sentakkiJudgementText;

        public DrawableSentakkiJudgement(JudgementResult result, DrawableSentakkiHitObject judgedObject)
            : base(result, judgedObject)
        {
        }

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager settings)
        {
            JudgementBody.Child = sentakkiJudgementText = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = Result.Type.GetDescription().ToUpperInvariant(),
                Font = OsuFont.Numeric.With(size: 20),
                Colour = colours.ForHitResult(Result.Type),
                Scale = new Vector2(0.85f, 1),
            };

            if (settings != null && settings.Get<bool>(SentakkiRulesetSettings.MaimaiJudgements))
            {
                switch (Result.Type)
                {
                    case HitResult.Perfect:
                        sentakkiJudgementText.Text = "Critical";
                        sentakkiJudgementText.Colour = Color4.Yellow;
                        break;

                    case HitResult.Great:
                        sentakkiJudgementText.Text = "Perfect";
                        break;

                    case HitResult.Good:
                        sentakkiJudgementText.Text = "Great";
                        break;

                    case HitResult.Meh:
                        sentakkiJudgementText.Text = "Good";
                        break;
                }
            }
        }
    }
}
