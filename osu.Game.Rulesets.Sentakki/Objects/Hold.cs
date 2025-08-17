using System.Threading;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
using osu.Game.Rulesets.Sentakki.Scoring;

namespace osu.Game.Rulesets.Sentakki.Objects;

public class Hold : SentakkiLanedHitObject, IHasDuration
{
    protected override bool PlaysBreakSample => false;

    public double EndTime
    {
        get => StartTime + Duration;
        set => Duration = value - StartTime;
    }

    public double Duration { get; set; }

    protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
    {
        AddNested(new HoldHead
        {
            Break = Break,
            StartTime = StartTime,
            Lane = Lane,
            Samples = Samples,
            Ex = Ex
        });

        base.CreateNestedHitObjects(cancellationToken);
    }

    protected override HitWindows CreateHitWindows() => new SentakkiHoldReleaseWindows();

    public class HoldHead : SentakkiLanedHitObject
    {
        public override Judgement CreateJudgement() => new SentakkiJudgement();
        protected override HitWindows CreateHitWindows() => new SentakkiTapHitWindows();
    }
}
