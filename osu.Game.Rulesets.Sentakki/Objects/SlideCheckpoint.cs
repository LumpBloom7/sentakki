using System.Collections.Generic;
using System.Threading;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects;

public class SlideCheckpoint : SentakkiHitObject
{
    public double SlideDuration { get; set; }
    public override double MaximumJudgementOffset => SlideDuration;

    // Used to update slides visuals
    public float Progress { get; set; }

    // The list of nodes to check for this checkpoint
    public List<Vector2> NodePositions { get; set; } = [];

    public int NodesToPass { get; set; } = 1;

    protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
    {
        foreach (var nodePosition in NodePositions)
            AddNested(new CheckpointNode(nodePosition) { StartTime = StartTime, SlideDuration = SlideDuration });
    }

    protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    public override Judgement CreateJudgement() => new IgnoreJudgement();

    public class CheckpointNode : SentakkiHitObject
    {
        public double SlideDuration { get; set; }
        public override double MaximumJudgementOffset => SlideDuration;

        public CheckpointNode(Vector2 position)
        {
            Position = position;
        }

        public readonly Vector2 Position;

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
        public override Judgement CreateJudgement() => new IgnoreJudgement();
    }
}
