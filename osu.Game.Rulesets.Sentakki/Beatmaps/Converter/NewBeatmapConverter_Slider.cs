using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

public partial class NewBeatmapConverter
{
    private SentakkiHitObject convertSlider(HitObject original, HitObject? previous, HitObject? next)
    {
        double duration = ((IHasDuration)original).Duration;

        bool stacked = isStack(original, previous);
        bool inStream = isStream(original, previous);

        var slider = (IHasPathWithRepeats)original;

        bool isBreak = slider.NodeSamples[0].Any(s => s.Name == HitSampleInfo.HIT_FINISH);
        bool isSpecial = slider.NodeSamples[0].Any(s => s.Name == HitSampleInfo.HIT_WHISTLE);

        if (stacked || !inStream)
            streamDirection = null;
        else
            streamDirection = getStreamDirection(original, previous, next);

        int streamOffset = streamDirection == RotationDirection.Clockwise ? 1 : -1;

        int lane = stacked
            ? currentLane
            : inStream
                ? currentLane + streamOffset
                : patternGenerator.RNG.Next(8);

        return new Hold()
        {
            Lane = currentLane = lane.NormalizePath(),
            Break = isBreak,
            StartTime = original.StartTime,
            Duration = duration,
            NodeSamples = slider.NodeSamples,
        };
    }
}
