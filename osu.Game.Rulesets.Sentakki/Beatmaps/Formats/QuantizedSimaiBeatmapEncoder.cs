using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Formats;

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
                    int divisorForTimeDiff = beatmap.ControlPointInfo.GetClosestBeatDivisor(timeUntilTimingPoint + prevTimingPoint.Time);
                    double subBeat = prevTimingPoint.BeatLength / divisorForTimeDiff;
                    maidataUnits.Add(new DivisorUnit(divisorForTimeDiff * timingPoint.TimeSignature.Numerator));

                    while (timeUntilTimingPoint >= subBeat)
                    {
                        maidataUnits.Add(new BeatUnit());
                        timeUntilTimingPoint -= subBeat;
                        currentTime += subBeat;
                    }
                }
            }

            maidataUnits.Add(new BPMUnit(bpm));

            for (int j = 0; j < hitObjectGroups.Count; ++j)
            {
                var hitObjectGroup = hitObjectGroups[j];
                // Handle time before hitObject time
                double timeUntilCurrent = hitObjectGroups[j].Time - currentTime;

                if (timeUntilCurrent > 5)
                {
                    int divisorForFirstObject = beatmap.ControlPointInfo.GetClosestBeatDivisor(timeUntilCurrent + timingPoint.Time);
                    double subBeat = beatLength / divisorForFirstObject;

                    maidataUnits.Add(new DivisorUnit(divisorForFirstObject * timingPoint.TimeSignature.Numerator));

                    bool beatAfterDivisor = false;

                    while (hitObjectGroups[j].Time - currentTime + 5 >= subBeat)
                    {
                        maidataUnits.Add(new BeatUnit());
                        timeUntilCurrent -= subBeat;
                        currentTime += subBeat;
                        beatAfterDivisor = true;
                    }

                    Debug.Assert(beatAfterDivisor);
                }

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

        // Ensure all divisor units are before hitObject units
        for (int i = 0; i < maidataUnits.Count - 1; ++i)
        {
            if (maidataUnits[i] is HitObjectCollectionUnit && maidataUnits[i + 1] is DivisorUnit)
            {
                var tmp = maidataUnits[i];
                maidataUnits[i] = maidataUnits[i + 1];
                maidataUnits[i + 1] = tmp;
            }
        }

        // Clean out redundant divisor units
        double currentBPM = 0;
        int currentDivisor = 0;

        List<IMaidataUnit> cleanMaiDataUnits = [];
        foreach (var unit in maidataUnits)
        {
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

            cleanMaiDataUnits.Add(unit);
        }

        StringBuilder maichartBuilder = new();
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

    private record struct BPMUnit(double bpm) : IMaidataUnit
    {
        public override readonly string ToString() => $"\n({bpm})";
    }

    private record struct DivisorUnit(int divisor) : IMaidataUnit
    {
        public override readonly string ToString() => $"\n{{{divisor}}}";
    }

    private record struct BeatTimeUnit(double beatLength) : IMaidataUnit
    {
        public override readonly string ToString() => $"\n({tempo}){{{divisor}}}";


        public double tempo = 60000 / beatLength;
        public float divisor = 4;
    }
    private record struct HitObjectCollectionUnit(string collectionString) : IMaidataUnit
    {
        public override readonly string ToString() => collectionString;
    }
}
