using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Extensions.EnumExtensions;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.Timing;
using osu.Game.IO;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using SimaiSharp;
using SimaiSharp.Structures;
using SlideSegment = osu.Game.Rulesets.Sentakki.Objects.SlidePath.SlideSegment;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Formats;

public class LegacySimaiBeatmapDecoder : LegacyBeatmapDecoder
{
    public new const int LATEST_VERSION = 1;

    public new static void Register()
    {
        AddDecoder<Beatmap>("sentakki file format - simai v", m => new LegacySimaiBeatmapDecoder(Parsing.ParseInt(m.Split("v").Last()))); // SIMAI chart starts with &
    }

    public LegacySimaiBeatmapDecoder(int version = LATEST_VERSION)
        : base(version)
    {
        locationPositionMap.Add(new Location { group = NoteGroup.CSensor, index = 0 }, new Vector2(0, 0));

        foreach ((float angle, int index) in SentakkiPlayfield.LANEANGLES.Select((angle, index) => (angle, index)))
        {
            locationPositionMap.Add(new Location { group = NoteGroup.BSensor, index = index }, MathExtensions.PointOnCircle(130, angle));
            locationPositionMap.Add(new Location { group = NoteGroup.ESensor, index = index }, MathExtensions.PointOnCircle(190, angle - 22.5f));
            locationPositionMap.Add(new Location { group = NoteGroup.ASensor, index = index }, MathExtensions.PointOnCircle(270, angle));
            locationPositionMap.Add(new Location { group = NoteGroup.DSensor, index = index }, MathExtensions.PointOnCircle(270, angle - 22.5f));
        }
    }

    private readonly List<string> noteLines = [];
    private readonly Dictionary<Location, Vector2> locationPositionMap = [];

    protected override void ParseLine(Beatmap beatmap, Section section, string line)
    {
        if (section != Section.HitObjects)
        {
            if (line.StartsWith("Mode", StringComparison.Ordinal))
            {
                beatmap.BeatmapInfo.Ruleset = new SentakkiRuleset().RulesetInfo;
                return;
            }

            base.ParseLine(beatmap, section, line);
            return;
        }

        noteLines.Add(line);
    }

    protected override void ParseStreamInto(LineBufferedReader stream, Beatmap output)
    {
        base.ParseStreamInto(stream, output);
        processNotes(output, string.Join('\n', noteLines));
    }

    private void attachSample(SentakkiHitObject hitObject)
    {
        HitSampleInfo normal = new HitSampleInfo(HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT);
        HitSampleInfo soft = new HitSampleInfo(HitSampleInfo.HIT_WHISTLE, HitSampleInfo.BANK_SOFT);

        hitObject.Samples.Add(normal);

        if (hitObject.Ex)
            hitObject.Samples.Add(soft);
    }

    private void processNotes(Beatmap beatmap, string chartNotes)
    {
        MaiChart chart = SimaiConvert.Deserialize(chartNotes);

        List<TimingControlPoint> simaiTimingPoints = [];

        TimingControlPoint? lastTimingPoint = null;

        foreach (var timingChange in chart.TimingChanges)
        {
            if (timingChange.TempoInherited)
                continue;

            double coercedTime = timingChange.time >= 0f
                ? timingChange.time
                : timingChange.time - Math.Floor(timingChange.time / timingChange.SecondsPerBar) * timingChange.SecondsPerBar;

            var controlPoint = new TimingControlPoint
            {
                Time = coercedTime * 1000f,
                BeatLength = 60000.0 / timingChange.trueInheritedTempo,
                TimeSignature = new TimeSignature(4)
            };

            lastTimingPoint = controlPoint;

            simaiTimingPoints.Add(controlPoint);
        }

        // Order them so that binary searches work.
        simaiTimingPoints = [.. simaiTimingPoints.OrderBy(s => s.Time)];

        if (chart.FinishTiming != null)
        {
            beatmap.BeatmapInfo.Length = (double)(chart.FinishTiming * 1000f);
        }

        List<EffectControlPoint> effectControlPoints = [];

        HashSet<TimingControlPoint> usedTimingPoints = [];

        foreach (NoteCollection noteCollection in chart.NoteCollections)
        {
            double roundedStartTime = noteCollection.time * 1000;

            var timingPoint = ControlPointInfo.BinarySearchWithFallback(simaiTimingPoints, roundedStartTime, null);
            if (timingPoint is not null)
                usedTimingPoints.Add(timingPoint);

            HashSet<double> hanabiTimes = [];

            foreach (var note in noteCollection)
            {
                if (note.styles.HasFlagFast(NoteStyles.Fireworks))
                    hanabiTimes.Add(roundedStartTime + (note.length ?? 0) * 1000.0);

                var hitObject = noteToHitObject(roundedStartTime, note);

                if (hitObject == null) continue;

                attachSample(hitObject);
                beatmap.HitObjects.Add(hitObject);
            }

            foreach (double time in hanabiTimes)
            {
                effectControlPoints.Add(new EffectControlPoint { Time = time, KiaiMode = true });
                effectControlPoints.Add(new EffectControlPoint { Time = time + 16, KiaiMode = false });
            }
        }

        simaiTimingPoints.RemoveAll(s => usedTimingPoints.Contains(s));

        foreach (var t in usedTimingPoints)
            beatmap.ControlPointInfo.Add(t!.Time, t);

        foreach (var e in effectControlPoints)
            beatmap.ControlPointInfo.Add(e.Time, e);

        autoGenerateBreaks(beatmap);
    }

    private int locationToLane(Location location)
    {
        return location.index;
    }

    private Vector2 locationToPosition(Location location)
    {
        return locationPositionMap[location];
    }

    private SentakkiHitObject? noteToHitObject(double time, Note note)
    {
        bool isBreak = note.type == NoteType.Break;
        NoteType senNoteType;

        if (note.IsStar || note.slidePaths.Count > 0)
        {
            senNoteType = NoteType.Slide;
        }
        else if (note.location.group != NoteGroup.Tap)
        {
            senNoteType = NoteType.Touch;
        }
        else
        {
            senNoteType = note.length != null ? NoteType.Hold : NoteType.Tap;
        }

        switch (senNoteType)
        {
            case NoteType.Tap:
                return new Tap
                {
                    Lane = locationToLane(note.location),
                    StartTime = time,
                    Break = isBreak,
                    Ex = note.IsEx,
                };

            case NoteType.Hold:
                Debug.Assert(note.length != null, "Length of hold note is not null");
                return new Hold
                {
                    Lane = locationToLane(note.location),
                    StartTime = time,
                    Duration = note.length.Value * 1000f,
                    Break = isBreak,
                    Ex = note.IsEx,
                };

            case NoteType.Touch:
                if (note.length != null)
                {
                    return new TouchHold
                    {
                        StartTime = time,
                        Duration = note.length.Value * 1000f,
                        Position = locationToPosition(note.location),
                        Break = isBreak,
                    };
                }

                return new Touch
                {
                    Position = locationToPosition(note.location),
                    StartTime = time,
                    Break = isBreak,
                    Ex = note.IsEx,
                };

            case NoteType.Slide:
                Slide slide = new Slide
                {
                    Lane = locationToLane(note.location),
                    StartTime = time,
                    Break = isBreak,
                    Ex = note.IsEx,
                };

                if (note.type is NoteType.ForceInvalidate)
                    slide.TapType = Slide.TapTypeEnum.None;

                if (note.appearance is NoteAppearance.ForceNormal)
                    slide.TapType = Slide.TapTypeEnum.Tap;

                attachSlideBodies(slide, note);
                return slide;
        }

        return null;
    }

    private void attachSlideBodies(Slide slide, Note note)
    {
        foreach (SlidePath path in note.slidePaths)
        {
            List<SlideSegment> segments = [];
            int startLane = locationToLane(note.location);

            foreach (var slideSegment in path.segments)
            {
                PathShape shape = PathShape.Straight;
                bool mirrored = false;

                int endLane;

                if (slideSegment.slideType == SlideType.EdgeFold)
                {
                    // Special logic to convert it into straights
                    foreach (var nextLocation in slideSegment.vertices)
                    {
                        endLane = locationToLane(nextLocation);
                        segments.Add(new SlideSegment(PathShape.Straight,
                            (endLane - startLane).NormalizeLane(),
                            false
                        ));
                        startLane = endLane;
                    }

                    continue;
                }

                switch (slideSegment.slideType)
                {
                    case SlideType.StraightLine:
                        break;

                    case SlideType.RingCw:
                    case SlideType.RingCcw:
                        shape = PathShape.Circle;
                        mirrored = slideSegment.slideType == SlideType.RingCcw;
                        break;

                    case SlideType.Fold:
                        shape = PathShape.V;
                        break;

                    case SlideType.CurveCw:
                    case SlideType.CurveCcw:
                        shape = PathShape.U;
                        mirrored = slideSegment.slideType == SlideType.CurveCw;
                        break;

                    case SlideType.ZigZagS:
                    case SlideType.ZigZagZ:
                        shape = PathShape.Thunder;
                        mirrored = slideSegment.slideType == SlideType.ZigZagZ;
                        break;

                    case SlideType.EdgeCurveCw:
                    case SlideType.EdgeCurveCcw:
                        shape = PathShape.Cup;
                        mirrored = slideSegment.slideType == SlideType.EdgeCurveCw;
                        break;

                    case SlideType.Fan:
                        shape = PathShape.Fan;
                        break;
                }

                endLane = locationToLane(slideSegment.vertices.Last());
                segments.Add(new SlideSegment(shape,
                    (endLane - startLane).NormalizeLane(),
                    mirrored
                ));
                startLane = endLane;
            }

            slide.SlideInfoList.Add(new SlideBodyInfo
            {
                Segments = segments,
                Duration = (path.delay + path.duration) * 1000f,
                Break = path.type == NoteType.Break,
                WaitDuration = path.delay * 1000f
            });
        }
    }

    private void autoGenerateBreaks(IBeatmap beatmap)
    {
        double currentMaxEndTime = double.MinValue;

        for (int i = 1; i < beatmap.HitObjects.Count; ++i)
        {
            var previousObject = beatmap.HitObjects[i - 1];
            var nextObject = beatmap.HitObjects[i];

            // Keep track of the maximum end time encountered thus far.
            // This handles cases like osu!mania's hold notes, which could have concurrent other objects after their start time.
            // Note that we're relying on the implicit assumption that objects are sorted by start time,
            // which is why similar tracking is not done for start time.
            currentMaxEndTime = Math.Max(currentMaxEndTime, previousObject.GetEndTime());

            if (nextObject.StartTime - currentMaxEndTime < BreakPeriod.MIN_GAP_DURATION)
                continue;

            double breakStartTime = currentMaxEndTime + BreakPeriod.GAP_BEFORE_BREAK;

            double breakEndTime = nextObject.StartTime;

            breakEndTime -= Math.Max(BreakPeriod.GAP_AFTER_BREAK, beatmap.ControlPointInfo.TimingPointAt(nextObject.StartTime).BeatLength * 2);

            if (breakEndTime - breakStartTime < BreakPeriod.MIN_BREAK_DURATION)
                continue;

            var breakPeriod = new BreakPeriod(breakStartTime, breakEndTime);

            if (beatmap.Breaks.Any(b => b.Intersects(breakPeriod)))
                continue;

            beatmap.Breaks.Add(breakPeriod);
        }
    }
}
