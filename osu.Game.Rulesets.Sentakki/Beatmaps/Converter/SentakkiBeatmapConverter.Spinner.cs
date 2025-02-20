using System.Linq;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

public partial class SentakkiBeatmapConverter
{
    private SentakkiHitObject convertSpinner(HitObject original)
    {
        bool isBreak = original.Samples.Any(s => s.Name == HitSampleInfo.HIT_FINISH);
        return new TouchHold
        {
            StartTime = original.StartTime,
            Samples = original.Samples,
            Duration = ((IHasDuration)original).Duration,
            Break = isBreak,
        };
    }
}
