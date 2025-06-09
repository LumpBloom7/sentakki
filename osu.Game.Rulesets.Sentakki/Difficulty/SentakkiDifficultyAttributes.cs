using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Sentakki.Difficulty
{
    public class SentakkiDifficultyAttributes : DifficultyAttributes
    {
        public override IEnumerable<(int attributeId, object value)> ToDatabaseAttributes()
        {
            foreach (var attribute in base.ToDatabaseAttributes())
                yield return attribute;

            yield return (ATTRIB_ID_DIFFICULTY, StarRating);
        }

        public override void FromDatabaseAttributes(IReadOnlyDictionary<int, double> values, IBeatmapOnlineInfo onlineInfo)
        {
            base.FromDatabaseAttributes(values, onlineInfo);

            StarRating = values[ATTRIB_ID_DIFFICULTY];
        }
    }
}
