using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Skinning;
using osu.Game.Rulesets.Sentakki.Skinning.Default;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI;

public partial class DrawableSentakkiJudgement : DrawableJudgement
{
    public DrawableSentakkiJudgement()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
    }

    public override void Apply(JudgementResult result, DrawableHitObject? judgedObject)
    {
        base.Apply(result, judgedObject);

        if (judgedObject is null)
            return;

        if (JudgementBody?.Drawable is IHasTimingIndicator judgementWithTimingIndicator)
            judgementWithTimingIndicator.ApplyTimingIndicatorFor(result);

        switch (judgedObject.HitObject)
        {
            case SentakkiLanedHitObject laned:
                Position = SentakkiExtensions.GetPositionAlongLane(240, laned.Lane);
                Rotation = laned.Lane.GetRotationForLane();
                break;

            // Special case for TouchHold's initial hit: We shift it upwards to improve visibility
            case TouchHold.TouchHoldHead touchHoldHead:
                Position = touchHoldHead.Position - new Vector2(0, 80);
                Rotation = 0;
                break;

            case IHasPosition touch:
                Position = touch.Position;
                Rotation = 0;
                break;
        }
    }

    protected override Drawable CreateDefaultJudgement(HitResult result) => new DefaultSentakkiJudgementPiece(result);
}
