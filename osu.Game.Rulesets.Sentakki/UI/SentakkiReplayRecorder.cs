using System.Collections.Generic;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Sentakki.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI;

public partial class SentakkiReplayRecorder : ReplayRecorder<SentakkiAction>
{
    public SentakkiReplayRecorder(Score score, DrawableSentakkiRuleset ruleset)
        : base(score)
    {
    }

    protected override ReplayFrame HandleFrame(Vector2 mousePosition, List<SentakkiAction> actions, ReplayFrame previousFrame)
        => new SentakkiReplayFrame(Time.Current, mousePosition, actions);
}
