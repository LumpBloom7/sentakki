using System.Collections.Generic;
using osu.Game.Replays;
using osu.Game.Rulesets.Sentakki.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.UI;
using osuTK;
using System;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class SentakkiReplayRecorder : ReplayRecorder<SentakkiAction>
    {
        public SentakkiReplayRecorder(Replay replay)
            : base(replay)
        {
        }

        protected override ReplayFrame HandleFrame(Vector2 mousePosition, List<SentakkiAction> actions, ReplayFrame previousFrame)
            => new SentakkiReplayFrame(Time.Current, mousePosition, Array.Empty<TouchReplayEvent>(), actions.ToArray());
    }
}
