using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Formats;

public class SimaiBeatmap
{
    internal static Dictionary<Vector2, string> PositionsToLocations = SentakkiPlayfield
        .LANEANGLES.SelectMany<float, KeyValuePair<Vector2, string>>(
            (angle, index) =>

                [
                    new(
                        SentakkiExtensions.GetCircularPosition(130, angle),
                        $"B{index+1}"
                    ),
                    new(
                        SentakkiExtensions.GetCircularPosition(190, angle - 22.5f),
                        $"E{index+1}"
                    ),
                    new(
                        SentakkiExtensions.GetCircularPosition(270, angle),
                        $"A{index+1}"
                    ),
                    new(
                        SentakkiExtensions.GetCircularPosition(270, angle - 22.5f),
                        $"D{index+1}"
                    ),
                ]
        )
        .Append(new(new(0, 0), "C"))
        .ToDictionary();

    private Beatmap<SentakkiHitObject> beatmap;

    public SimaiBeatmap(Beatmap<SentakkiHitObject> beatmap)
    {
        this.beatmap = beatmap;

        maidata = maidataFromSentakkiBeatmap2(beatmap);
    }

    private string maidata = "";

    private string maidataFromSentakkiBeatmap2(Beatmap<SentakkiHitObject> beatmap)
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
                        Tap t => tapToString(t),
                        Hold h => holdToString(h),
                        Slide s => slideToString(s),
                        Touch tc => touchToString(tc),
                        TouchHold th => touchHoldToString(th),
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

        maichartBuilder.Append('E');

        return maichartBuilder.ToString();
    }

    private string maidataFromSentakkiBeatmap(Beatmap<SentakkiHitObject> beatmap)
    {
        var hitObjectsGroups = beatmap.HitObjects.GroupBy(h => h.StartTime).OrderBy(g => g.Key).ToList();

        if (hitObjectsGroups.Count == 0)
            return "E";

        StringBuilder maidataBuilder = new();

        // Add padding timingPoint prior to first hitobject
        if (hitObjectsGroups[0].Key > 0)
            maidataBuilder.Append($"\n{{#{hitObjectsGroups[0].Key / 1000:F3}}},");

        for (int i = 0; i < hitObjectsGroups.Count; ++i)
        {
            var group = hitObjectsGroups[i];

            if (i < hitObjectsGroups.Count - 1)
            {
                var nextGroup = hitObjectsGroups[i + 1];
                double delta = nextGroup.Key - group.Key;

                maidataBuilder.Append($"\n{{#{delta / 1000:F3}}}");
            }

            var hitobjects = group.ToList();
            for (int j = 0; j < hitobjects.Count; ++j)
            {
                var hitobject = hitobjects[j];

                string hitObjectString = hitobject switch
                {
                    Tap t => tapToString(t),
                    Hold h => holdToString(h),
                    Slide s => slideToString(s),
                    Touch tc => touchToString(tc),
                    TouchHold th => touchHoldToString(th),
                    _ => ""
                };

                maidataBuilder.Append(hitObjectString);
                if (j < hitobjects.Count - 1)
                    maidataBuilder.Append('/');
            }

            maidataBuilder.Append(',');
        }

        maidataBuilder.Append('E');

        return maidataBuilder.ToString();
    }

    private static string tapToString(Tap tap) => $"{tap.Lane + 1}{(tap.Break ? "b" : "")}{(tap.Ex ? "x" : "")}";
    private static string holdToString(Hold hold) => $"{hold.Lane + 1}h{(hold.Break ? "b" : "")}{(hold.Ex ? "x" : "")}[#{hold.Duration / 1000:F3}]";

    private string slideToString(Slide slide)
    {
        StringBuilder slideBuilder = new();

        slideBuilder.Append($"{slide.Lane + 1}");

        // Tap has break
        if (slide.Break)
            slideBuilder.Append('b');

        // Tap no star
        if (slide.TapType == Slide.TapTypeEnum.Star)
            slideBuilder.Append("$$");
        else if (slide.TapType == Slide.TapTypeEnum.Tap)
            slideBuilder.Append('@');
        else if (slide.TapType == Slide.TapTypeEnum.None)
            slideBuilder.Append('?');

        // Tap EX
        if (slide.Ex)
            slideBuilder.Append('x');

        if (slide.SlideInfoList.Count > 0)
        {
            for (int i = 0; i < slide.SlideInfoList.Count; ++i)
            {
                var slideInfo = slide.SlideInfoList[i];
                int currentLane = slide.Lane;
                foreach (var part in slideInfo.SlidePathParts)
                {
                    int endLane = (currentLane + part.EndOffset).NormalizePath();
                    slideBuilder.Append(shapeForSlidePart(currentLane, endLane, part));
                    slideBuilder.Append(endLane + 1);
                    currentLane = endLane;
                }

                double millisPerBeat = beatmap.ControlPointInfo.TimingPointAt(slide.StartTime).BeatLength;
                double shootDelayMs = slideInfo.ShootDelay * millisPerBeat;

                double durationWithoutDelay = slideInfo.Duration - shootDelayMs;

                slideBuilder.Append($"[{shootDelayMs / 1000:F3}##{durationWithoutDelay / 1000:F3}]");
                if (slideInfo.Break)
                    slideBuilder.Append('b');

                if (i < slide.SlideInfoList.Count - 1)
                    slideBuilder.Append('*');
            }
        }

        return slideBuilder.ToString();
    }

    private static string touchToString(Touch touch) => PositionsToLocations.MinBy(kv =>
                                                    {
                                                        double xDelta = Math.Abs(kv.Key.X - touch.Position.X);
                                                        double yDelta = Math.Abs(kv.Key.Y - touch.Position.Y);
                                                        return Math.Sqrt(xDelta * xDelta + yDelta * yDelta);
                                                    }).Value;

    private static string touchHoldToString(TouchHold touchHold) => $"C[#{touchHold.Duration / 1000:F3}]";

    private static string shapeForSlidePart(int startLane, int endLane, SlideBodyPart part)
    {
        switch (part.Shape)
        {
            case SlidePaths.PathShapes.Straight:
                return "-";

            case SlidePaths.PathShapes.Circle:
                bool startsFromBottom = ((startLane + 2) % 8) >= 4;
                bool facingLeft = part.Mirrored ^ startsFromBottom;

                return facingLeft ? "<" : ">";

            case SlidePaths.PathShapes.V:
                return "v";

            case SlidePaths.PathShapes.U:
                return part.Mirrored ? "q" : "p";

            case SlidePaths.PathShapes.Thunder:
                return part.Mirrored ? "z" : "s";

            case SlidePaths.PathShapes.Cup:
                return part.Mirrored ? "qq" : "pp";

            case SlidePaths.PathShapes.Fan:
                return "w";

            default:
                return "-";
        }
    }

    public void SerializeToFile()
    {
        var metadata = beatmap.BeatmapInfo.Metadata;
        string path = $"(sen) {metadata.ArtistUnicode} - {metadata.TitleUnicode} ({beatmap.BeatmapInfo.DifficultyName}).txt";
        StringBuilder fileContentBuilder = new();
        fileContentBuilder.AppendLine($"&title={metadata.TitleUnicode}");
        fileContentBuilder.AppendLine($"&artist={metadata.ArtistUnicode}");
        fileContentBuilder.AppendLine($"&wholebpm={Math.Round(beatmap.BeatmapInfo.BPM)}");
        fileContentBuilder.AppendLine($"&lv_7=洗");
        fileContentBuilder.AppendLine($"&inote_7={maidata}");

        var file = File.CreateText(path);
        file.Write(fileContentBuilder);
        file.Close();
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
        public override readonly string ToString() => $"\n{{#{beatLength / 1000}}}";

        public double bpm = 60000 / beatLength;
        public float divisor = 1;
    }
    private record struct HitObjectCollectionUnit(string collectionString) : IMaidataUnit
    {
        public override readonly string ToString() => collectionString;
    }
}
