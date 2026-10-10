using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Extensions;
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

        switch (judgedObject)
        {
            case DrawableSentakkiLanedHitObject laned:
                Position = SentakkiExtensions.GetPositionAlongLane(240, laned.HitObject.Lane);
                Rotation = laned.HitObject.Lane.GetRotationForLane();
                break;

            case DrawableTouchHold touchHold:
                Position = touchHold.Position;
                Rotation = 0;
                break;

            case DrawableTouchHoldHead touchHoldHead:
                Position = touchHoldHead.Position - new Vector2(0, 80);
                Rotation = 0;
                break;
        }
    }

    protected override Drawable CreateDefaultJudgement(HitResult result) => new DefaultSentakkiJudgementPiece(result);
}
