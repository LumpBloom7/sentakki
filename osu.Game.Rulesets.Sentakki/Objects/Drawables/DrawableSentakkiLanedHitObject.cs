using System;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.UI;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables;

public partial class DrawableSentakkiLanedHitObject : DrawableSentakkiHitObject
{
    public new SentakkiLanedHitObject HitObject => (SentakkiLanedHitObject)base.HitObject;

    protected override float SamplePlaybackPosition =>
        SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, HitObject.Lane).X / (SentakkiPlayfield.INTERSECTDISTANCE * 2) + .5f;

    public DrawableSentakkiLanedHitObject(SentakkiLanedHitObject? hitObject)
        : base(hitObject)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        if (DrawableSentakkiRuleset is not null)
            AnimationDuration.BindTo(DrawableSentakkiRuleset?.AdjustedAnimDuration);
    }

    protected bool IsValidLaneAction(SentakkiAction action)
    {
        int laneNumber = HitObject.Lane;

        Console.WriteLine(action);

        return
            action == (SentakkiAction.B1Lane1 + laneNumber)
            || action == (SentakkiAction.B2Lane1 + laneNumber)
            || action == (SentakkiAction.SensorLane1 + laneNumber)
            || action == (SentakkiAction.Key1 + laneNumber);
    }
}
