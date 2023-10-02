using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

public partial class SentakkiBeatmapConverter
{
    private SentakkiHitObject convertSpinner(HitObject original)
    {
        return new TouchHold
        {
            StartTime = original.StartTime,
            Samples = original.Samples,
            Duration = ((IHasDuration)original).Duration
        };
    }
}
