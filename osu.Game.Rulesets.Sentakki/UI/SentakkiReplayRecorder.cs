using System.Collections.Generic;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Sentakki.Replays;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class SentakkiReplayRecorder : ReplayRecorder<SentakkiAction>
    {
        private DrawableSentakkiRuleset drawableRuleset;

        public SentakkiReplayRecorder(Replay replay, DrawableSentakkiRuleset ruleset)
            : base(replay)
        {
            drawableRuleset = ruleset;
        }

        protected override ReplayFrame HandleFrame(Vector2 mousePosition, List<SentakkiAction> actions, ReplayFrame previousFrame)
            => new SentakkiReplayFrame(Time.Current, mousePosition, drawableRuleset.UseSensorMode, actions.ToArray());
    }
}
