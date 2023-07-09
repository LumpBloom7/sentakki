using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

public partial class NewBeatmapConverter
{
    private ConversionExperiments conversionExperiments;

    private int currentLane;

    private readonly double timePreempt;
    private readonly float circleRadius;

    private readonly IBeatmap beatmap;

    private readonly SentakkiPatternGenerator patternGenerator;

    public NewBeatmapConverter(IBeatmap beatmap, ConversionExperiments experiments)
    {
        this.beatmap = beatmap;

        this.conversionExperiments = experiments;

        // Taking this from osu specific information that we need
        timePreempt = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.ApproachRate, 1800, 1200, 450);
        circleRadius = 54.4f - 4.48f * beatmap.Difficulty.CircleSize;

        patternGenerator = new SentakkiPatternGenerator(beatmap);
    }

    public IEnumerable<SentakkiHitObject> convertHitObject(HitObject original)
    {
        HitObject? previous = null;
        HitObject? next = null;

        for (int i = 0; i < beatmap.HitObjects.Count; ++i)
        {
            if (beatmap.HitObjects[i] != original) continue;

            if (i > 0)
                previous = beatmap.HitObjects[i - 1];
            if (i < beatmap.HitObjects.Count - 1)
                next = beatmap.HitObjects[i + 1];

            break;
        }

        switch (original)
        {
            case IHasPathWithRepeats:
                yield return convertSlider(original, previous, next);

                break;

            default:
                yield return convertHitCircle(original, previous, next);

                break;
        }
    }
}
