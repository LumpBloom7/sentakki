// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
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

        public DrawableSentakkiJudgement(JudgementResult result, DrawableSentakkiHitObject judgedObject)
            : base(result, judgedObject)
        {
        }

        [BackgroundDependencyLoader(true)]
        private void load(TextureStore textures, SentakkiRulesetConfigManager settings)
        {
            InternalChild = JudgementBody = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Child = JudgementText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = Result.Type.GetDescription().ToUpperInvariant(),
                    Font = OsuFont.Numeric.With(size: 20),
                    Colour = colours.ForHitResult(Result.Type),
                    Scale = new Vector2(0.85f, 1),
                }
            };
            if (settings != null && settings.Get<bool>(SentakkiRulesetSettings.MaimaiJudgements))
            {
                switch (Result.Type)
                {
                    case HitResult.Perfect:
                        JudgementText.Text = "Critical";
                        JudgementText.Colour = Color4.Yellow;
                        break;

                    case HitResult.Great:
                        JudgementText.Text = "Perfect";
                        break;

                    case HitResult.Good:
                        JudgementText.Text = "Great";
                        break;

                    case HitResult.Meh:
                        JudgementText.Text = "Good";
                        break;
                }
            }
        }

        protected override double FadeOutDelay => base.FadeOutDelay;

        protected override void ApplyHitAnimations()
        {
            JudgementText?.TransformSpacingTo(new Vector2(14, 0), 1800, Easing.OutQuint);
            base.ApplyHitAnimations();
        }
    }
}
