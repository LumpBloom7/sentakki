using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Users;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModAutoplay : ModAutoplay<SentakkiHitObject>, IApplicableToDrawableHitObjects
    {
        public override Score CreateReplayScore(IBeatmap beatmap) => new Score
        {
            ScoreInfo = new ScoreInfo
            {
                User = new User { Username = "Mai-chan" },
            },
            Replay = new SentakkiAutoGenerator(beatmap).Generate(),
        };

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var d in drawables.OfType<DrawableSentakkiHitObject>())
            {
                d.Auto = true;
            }
        }
    }
}
