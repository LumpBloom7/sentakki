using System.Linq;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

public partial class SentakkiBeatmapConverter
{
    private Tap convertHitCircle(HitObject original)
    {
        bool isBreak = original.Samples.Any(s => s.Name == HitSampleInfo.HIT_FINISH);

        Tap result = new Tap
        {
            Lane = currentLane.NormalizePath(),
            Samples = original.Samples,
            StartTime = original.StartTime,
            Break = isBreak
        };

        return result;
    }
}
