using System.Linq;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

public partial class SentakkiBeatmapConverter
{
    private Tap convertHitCircle(HitObject original) => convertHitCircle(original, currentLane, original.StartTime);
    private Tap convertHitCircle(HitObject original, int lane, double startTime)
    {
        bool isBreak = original.Samples.Any(s => s.Name == HitSampleInfo.HIT_FINISH);
        bool isSoft = original.Samples.Any(s => s.Name == HitSampleInfo.HIT_WHISTLE);

        Tap result = new Tap
        {
            Lane = lane,
            Samples = original.Samples,
            StartTime = startTime,
            Break = isBreak,
            Ex = isSoft
        };

        return result;
    }
}
