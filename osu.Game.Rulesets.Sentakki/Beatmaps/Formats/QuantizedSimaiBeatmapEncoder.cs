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
/// Alternative simai encoder that encodes the chart in idiomatic simai, using the beats and divisor notation of simai.
///
/// This encoder lossy due to osu/sentakki hitobjects not necessarily lining up to subbeats perfectly due to mapper error or floating-point error.
/// In practice this is not noticable as all notes will have at most 5ms deviation from their true timing.
///
/// The resulting simai chart is fully compatible with Majdata and AstroDX.
/// </summary>
public class QuantizedSimaiBeatmapEncoder : SimaiBeatmapEncoder
{
    public QuantizedSimaiBeatmapEncoder(IBeatmap<SentakkiHitObject> beatmap)
        : base(beatmap)
    {
    }

    protected override string CreateSimaiChart()
    {
        if (Beatmap.HitObjects.Count == 0)
            return "E";

        List<IMaidataUnit> maidataUnits = [];

        var timingGroups =
            Beatmap.HitObjects.GroupBy(h => h.StartTime)
                   .Select(g => new
                   {
                       Time = g.Key,
                       HitObjects = g.ToList()
                   })
                   .GroupBy(g => Beatmap.ControlPointInfo.TimingPointAt(g.Time)).Select(g => new
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
                    var beatsUntilTimingPoint = AsFraction(timeUntilTimingPoint / prevTimingPoint.BeatLength);

                    maidataUnits.AddRange(generatePaddingBeats(beatsUntilTimingPoint));
                    currentTime = timingPoint.Time;
                }
            }

            maidataUnits.Add(new BPMUnit(bpm));

            for (int j = 0; j < hitObjectGroups.Count; ++j)
            {
                var hitObjectGroup = hitObjectGroups[j];
                // Handle time before hitObject time
                double timeUntilCurrent = hitObjectGroups[j].Time - currentTime;

                var beatsUntilCurrent = AsFraction(timeUntilCurrent / timingPoint.BeatLength);

                maidataUnits.AddRange(generatePaddingBeats(beatsUntilCurrent));
                currentTime = hitObjectGroups[j].Time;

                StringBuilder hitObjectGroupBuilder = new StringBuilder();

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
                    if (currentBPM == b.BPM)
                        continue;

                    currentBPM = b.BPM;
                    currentDivisor = 1;

                    break;

                case DivisorUnit d:
                    if (currentDivisor == d.Divisor)
                        continue;

                    currentDivisor = d.Divisor;
                    break;

                case BeatTimeUnit bt:
                    currentDivisor = 0;
                    currentBPM = 0;
                    break;
            }

            if (cleanMaiDataUnits.Count > 0 && cleanMaiDataUnits[^1] is HitObjectCollectionUnit previousHoc)
            {
                switch (unit)
                {
                    // Merge hitobject collections without a beat in between
                    // This may happen due to the notes being at less 5 ms away from each other, which is beyond the precision of this encoder, and therefore the encoder doesn't generate padding beats
                    case HitObjectCollectionUnit currentHoc:
                        cleanMaiDataUnits[^1] = new HitObjectCollectionUnit(
                            $"{previousHoc.CollectionString}/{currentHoc.CollectionString}"
                        );
                        continue;

                    // Ensure all timing units are before hitObject units
                    case DivisorUnit or BeatTimeUnit or BPMUnit:
                        cleanMaiDataUnits[^1] = maidataUnits[i];
                        cleanMaiDataUnits.Add(previousHoc);
                        continue;
                }
            }

            cleanMaiDataUnits.Add(unit);
        }

        StringBuilder maichartBuilder = new StringBuilder();
        foreach (var item in cleanMaiDataUnits)
            maichartBuilder.Append(item);

        maichartBuilder.Append(",\nE");

        return maichartBuilder.ToString();
    }

    private interface IMaidataUnit
    {
        string ToString();
    }

    private record struct BeatUnit : IMaidataUnit
    {
        public override readonly string ToString() => ",";
    }

    private readonly record struct BPMUnit(double BPM) : IMaidataUnit
    {
        public override readonly string ToString() => Invariant($"\n({BPM})");
    }

    private readonly record struct DivisorUnit(int Divisor) : IMaidataUnit
    {
        public override readonly string ToString() => Invariant($"\n{{{Divisor}}}");
    }

    private readonly record struct BeatTimeUnit(double BeatLength) : IMaidataUnit
    {
        public override readonly string ToString() => Invariant($"\n({Tempo}){{{DIVISOR}}}");

        public readonly double Tempo = 60000 / BeatLength;
        public const int DIVISOR = 4;
    }

    private readonly record struct HitObjectCollectionUnit(string CollectionString) : IMaidataUnit
    {
        public override readonly string ToString() => CollectionString;
    }

    // Source: Algorithm To Convert A Decimal To A Fraction - John Kennedy (rkennedy@ix.netcom.com)
    // https://web.archive.org/web/20111027100847/http://homepage.smc.edu/kennedy_john/DEC2FRAC.pdf
    // Lightly adjusted to output whole numbers separately.
    // The accuracy is chosen so that the fraction will deviate from x by at most two decimal places.
    // x = interval / BeatLength, so in practice you wouldn't need too many decimal places
    public static (int wholes, int numerator, int denominator) AsFraction(double x, double maxError = 0.001)
    {
        if (x == 0)
            return (0, 0, 1);

        int sign = Math.Sign(x);
        x = Math.Abs(x);

        int n = (int)Math.Floor(x);
        x -= n;

        if (x < maxError)
            return new(sign * n, 0, 1);

        if (1 - maxError < x)
            return (sign * (n + 1), 0, 1);

        double z = x;
        int previousDenominator = 0;
        int denominator = 1;
        int numerator;

        do
        {
            z = 1.0 / (z - (int)z);
            int temp = denominator;
            denominator = denominator * (int)z + previousDenominator;
            previousDenominator = temp;
            numerator = Convert.ToInt32(x * denominator);
        }
        while (Math.Abs(x - (double)numerator / denominator) > maxError && z != (int)z);

        return (n * sign, numerator * sign, denominator);
    }

    private List<IMaidataUnit> generatePaddingBeats((int wholes, int numerator, int denominator) fraction)
    {
        List<IMaidataUnit> result = [];

        if (fraction.wholes > 0)
        {
            result.Add(new DivisorUnit(4));

            for (int i = 0; i < fraction.wholes; ++i)
                result.Add(new BeatUnit());
        }

        if (fraction.numerator > 0)
        {
            result.Add(new DivisorUnit(fraction.denominator * 4));

            for (int i = 0; i < fraction.numerator; ++i)
                result.Add(new BeatUnit());
        }

        return result;
    }
}
