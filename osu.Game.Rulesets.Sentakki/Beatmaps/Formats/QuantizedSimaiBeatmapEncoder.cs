using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;

using static System.FormattableString;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Formats;

/// <summary>
/// Alternativee simai encoder that encodes the chart in idiomatic simai, using the beats and divisor notation of simai.
///
/// This encoder lossy due to osu/sentakki hitobjects not necessarily lining up to subbeats perfectly due to mapper error or floating-point error.
/// In practice this is not noticable as all notes will have at most 5ms deviation from their true timing.
///
/// The resulting simai chart is fully compatible with Majdata and AstroDX.
/// </summary>
public class QuantizedSimaiBeatmapEncoder : SimaiBeatmapEncoder
{
    public QuantizedSimaiBeatmapEncoder(IBeatmap<SentakkiHitObject> beatmap) : base(beatmap) { }

    protected override string CreateSimaiChart()
    {
        if (beatmap.HitObjects.Count == 0)
            return "E";

        List<IMaidataUnit> maidataUnits = [];

        var timingGroups =
            beatmap.HitObjects.GroupBy(h => h.StartTime)
                .Select(g => new
                {
                    Time = g.Key,
                    HitObjects = g.ToList()
                })
                .GroupBy(g => beatmap.ControlPointInfo.TimingPointAt(g.Time)).Select(g => new
                {
                    TimingPoint = g.Key,
                    HitObjectGroups = g.ToList()
                }).OrderBy(g => g.TimingPoint.Time).ToList();

        double currentTime = 0;
        for (int i = 0; i < timingGroups.Count; ++i)
        {
            TimingControlPoint timingPoint = timingGroups[i].TimingPoint;

            var hitObjectGroups = timingGroups[i].HitObjectGroups;

            double bpm = timingPoint.BPM;
            double beatLength = timingPoint.BeatLength;

            double timeUntilTimingPoint = timingPoint.Time - currentTime;
            if (timeUntilTimingPoint > 0)
            {
                // First timing point, any time before this is lead in time
                if (i == 0)
                {
                    maidataUnits.Add(new BeatTimeUnit(timeUntilTimingPoint));
                    maidataUnits.Add(new BeatUnit());
                    currentTime = timingPoint.Time;
                }
                else
                {
                    var prevTimingPoint = timingGroups[i - 1].TimingPoint;
                    maidataUnits.AddRange(generatePaddingBeats(timeUntilTimingPoint, prevTimingPoint, out double timeRemaining));
                    currentTime = timingPoint.Time - timeRemaining;
                }
            }

            maidataUnits.Add(new BPMUnit(bpm));

            for (int j = 0; j < hitObjectGroups.Count; ++j)
            {
                var hitObjectGroup = hitObjectGroups[j];
                // Handle time before hitObject time
                double timeUntilCurrent = hitObjectGroups[j].Time - currentTime;

                maidataUnits.AddRange(generatePaddingBeats(timeUntilCurrent, timingPoint, out double timeRemaining));
                currentTime = hitObjectGroups[j].Time - timeRemaining;

                StringBuilder hitObjectGroupBuilder = new();

                // This is locked in
                for (int k = 0; k < hitObjectGroup.HitObjects.Count; ++k)
                {
                    var hitObject = hitObjectGroup.HitObjects[k];

                    string hitObjectString = hitObject switch
                    {
                        Tap t => TapToString(t),
                        Hold h => HoldToString(h),
                        Slide s => SlideToString(s),
                        Touch tc => TouchToString(tc),
                        TouchHold th => TouchHoldToString(th),
                        _ => ""
                    };
                    hitObjectGroupBuilder.Append(hitObjectString);
                    if (k < hitObjectGroup.HitObjects.Count - 1)
                        hitObjectGroupBuilder.Append('/');
                }
                maidataUnits.Add(new HitObjectCollectionUnit(hitObjectGroupBuilder.ToString()));
            }
        }

        List<IMaidataUnit> cleanMaiDataUnits = [];
        double currentBPM = 0;
        int currentDivisor = 0;

        for (int i = 0; i < maidataUnits.Count; ++i)
        {
            var unit = maidataUnits[i];

            // We attempt to immediately discard redundant timing points
            switch (unit)
            {
                case BPMUnit b:
                    if (currentBPM == b.bpm)
                        continue;

                    currentBPM = b.bpm;
                    currentDivisor = 1;

                    break;

                case DivisorUnit d:
                    if (currentDivisor == d.divisor)
                        continue;

                    currentDivisor = d.divisor;
                    break;

                case BeatTimeUnit bt:
                    currentDivisor = 0;
                    currentBPM = 0;
                    break;
            }

            if (i > 0 && maidataUnits[i - 1] is HitObjectCollectionUnit previousHOC)
            {
                // Merge hitobject collections without a beat in between
                // This may happen due to the notes being at less 5 ms away from each other, which is beyond the precision of this encoder, and therefore the encoder doesn't generate padding beats
                if (unit is HitObjectCollectionUnit currentHOC)
                {
                    cleanMaiDataUnits[^1] = new HitObjectCollectionUnit(
                    $"{previousHOC.collectionString}/{currentHOC.collectionString}"
                    );
                    continue;
                }
                // Ensure all timing units are before hitObject units
                else if (unit is DivisorUnit or BeatTimeUnit or BPMUnit)
                {
                    cleanMaiDataUnits[^1] = maidataUnits[i];
                    cleanMaiDataUnits.Add(previousHOC);
                    continue;
                }
            }

            cleanMaiDataUnits.Add(unit);
        }

        StringBuilder maichartBuilder = new();
        foreach (var item in cleanMaiDataUnits)
            maichartBuilder.Append(item);

        maichartBuilder.Append(",\nE");

        return maichartBuilder.ToString();
    }

    private static List<IMaidataUnit> generatePaddingBeats(double timeDelta, TimingControlPoint activeTimingPoint, out double timeRemaining)
    {
        timeRemaining = timeDelta;
        if (timeDelta < 5)
            return [];

        List<IMaidataUnit> paddingBeats = [];

        int currentDivisor = 1;

        while (timeRemaining > 5)
        {
            double subBeatLength = activeTimingPoint.BeatLength / currentDivisor;

            int numberOfSubBeats = (int)(timeRemaining / subBeatLength);

            // Is it possible to get within 5ms if we allow timeDelta to go negative?
            // If so, we would prefer that as it is "close enough" while reducing number of subdivisions
            if (Math.Abs(timeRemaining - subBeatLength * (numberOfSubBeats + 1)) < 5)
                ++numberOfSubBeats;

            if (numberOfSubBeats > 0)
            {
                paddingBeats.Add(new DivisorUnit(currentDivisor * 4));

                for (int i = 0; i < numberOfSubBeats; ++i)
                    paddingBeats.Add(new BeatUnit());
            }

            timeRemaining -= numberOfSubBeats * subBeatLength;
            currentDivisor <<= 1;
        }

        return paddingBeats;
    }

    private interface IMaidataUnit
    {
        string ToString();
    }

    private record struct BeatUnit : IMaidataUnit
    {
        public override readonly string ToString() => ",";
    }

    private record struct BPMUnit(double bpm) : IMaidataUnit
    {
        public override readonly string ToString() => Invariant($"\n({bpm})");
    }

    private record struct DivisorUnit(int divisor) : IMaidataUnit
    {
        public override readonly string ToString() => Invariant($"\n{{{divisor}}}");
    }

    private record struct BeatTimeUnit(double beatLength) : IMaidataUnit
    {
        public override readonly string ToString() => Invariant($"\n({tempo}){{{divisor}}}");

        public double tempo = 60000 / beatLength;
        public int divisor = 4;
    }
    private record struct HitObjectCollectionUnit(string collectionString) : IMaidataUnit
    {
        public override readonly string ToString() => collectionString;
    }
}
