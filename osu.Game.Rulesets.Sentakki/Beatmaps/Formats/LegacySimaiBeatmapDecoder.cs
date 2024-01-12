using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.Timing;
using osu.Game.IO;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using SimaiSharp;
using SimaiSharp.Structures;

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
            locationPositionMap.Add(new Location { group = NoteGroup.BSensor, index = index }, SentakkiExtensions.GetCircularPosition(130, angle));
            locationPositionMap.Add(new Location { group = NoteGroup.ESensor, index = index }, SentakkiExtensions.GetCircularPosition(190, angle - 22.5f));
            locationPositionMap.Add(new Location { group = NoteGroup.ASensor, index = index }, SentakkiExtensions.GetCircularPosition(270, angle));
            locationPositionMap.Add(new Location { group = NoteGroup.DSensor, index = index }, SentakkiExtensions.GetCircularPosition(270, angle - 22.5f));
        }
    }

    private readonly IList<string> noteLines = new List<string>();
    private readonly IDictionary<Location, Vector2> locationPositionMap = new Dictionary<Location, Vector2>();

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

    private void processNotes(Beatmap beatmap, string chartNotes)
    {
        MaiChart chart = SimaiConvert.Deserialize(chartNotes);

        foreach (var timingChange in chart.TimingChanges)
        {
            var controlPoint = new TimingControlPoint
            {
                Time = timingChange.time * 1000f,
                BeatLength = timingChange.SecondsPerBar * 1000f
            };

            if (timingChange.subdivisions != 0 && timingChange.subdivisions % 1 == 0)
            {
                controlPoint.TimeSignature = new TimeSignature((int)timingChange.subdivisions);
            }

            beatmap.ControlPointInfo.Add(controlPoint.Time, controlPoint);
        }

        if (chart.FinishTiming != null)
        {
            beatmap.BeatmapInfo.Length = (double)(chart.FinishTiming * 1000f);
        }

        foreach (NoteCollection noteCollection in chart.NoteCollections)
        {
            foreach (var note in noteCollection)
            {
                var hitObject = noteToHitObject(noteCollection.time * 1000f, note, beatmap.ControlPointInfo);
                if (hitObject != null)
                {
                    hitObject.Samples.Add(new HitSampleInfo(HitSampleInfo.HIT_NORMAL, hitObject.Ex ? HitSampleInfo.BANK_SOFT : HitSampleInfo.BANK_NORMAL));
                    beatmap.HitObjects.Add(hitObject);
                }
            }
        }
    }

    private int locationToLane(Location location)
    {
        return location.index;
    }

    private Vector2 locationToPosition(Location location)
    {
        return locationPositionMap[location];
    }

    private SentakkiHitObject? noteToHitObject(double time, Note note, ControlPointInfo controlPointInfo)
    {
        bool isBreak = note.type == NoteType.Break;

        if (note.IsStar || note.slidePaths.Count > 0)
        {
            note.type = NoteType.Slide;
        }
        else if (note.location.group != NoteGroup.Tap)
        {
            note.type = NoteType.Touch;
        }
        else
        {
            note.type = note.length != null ? NoteType.Hold : NoteType.Tap;
        }

        switch (note.type)
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
                    };
                }

                return new Touch
                {
                    Position = locationToPosition(note.location),
                    StartTime = time,
                };

            case NoteType.Slide:
                Slide slide = new Slide
                {
                    Lane = locationToLane(note.location),
                    StartTime = time,
                    Break = isBreak,
                    Ex = note.IsEx,
                };
                // Currently Sentakki's head of slide is always a star
                attachSlideBodies(slide, note, controlPointInfo);
                return slide;
        }

        return null;
    }

    private void attachSlideBodies(Slide slide, Note note, ControlPointInfo controlPointInfo)
    {
        foreach (SlidePath path in note.slidePaths)
        {
            List<SlideBodyPart> slideBodyParts = new List<SlideBodyPart>();
            int startLane = locationToLane(note.location);

            foreach (SlideSegment slideSegment in path.segments)
            {
                SlidePaths.PathShapes shape = SlidePaths.PathShapes.Straight;
                bool mirrored = false;

                int endLane;

                if (slideSegment.slideType == SlideType.EdgeFold)
                {
                    // Special logic to convert it into straights
                    foreach (var nextLocation in slideSegment.vertices)
                    {
                        endLane = locationToLane(nextLocation);
                        slideBodyParts.Add(new SlideBodyPart(SlidePaths.PathShapes.Straight,
                            (endLane - startLane).NormalizePath(),
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
                        shape = SlidePaths.PathShapes.Circle;
                        mirrored = slideSegment.slideType == SlideType.RingCcw;
                        break;

                    case SlideType.Fold:
                        shape = SlidePaths.PathShapes.V;
                        break;

                    case SlideType.CurveCw:
                    case SlideType.CurveCcw:
                        shape = SlidePaths.PathShapes.U;
                        mirrored = slideSegment.slideType == SlideType.CurveCw;
                        break;

                    case SlideType.ZigZagS:
                    case SlideType.ZigZagZ:
                        shape = SlidePaths.PathShapes.Thunder;
                        mirrored = slideSegment.slideType == SlideType.ZigZagZ;
                        break;

                    case SlideType.EdgeCurveCw:
                    case SlideType.EdgeCurveCcw:
                        shape = SlidePaths.PathShapes.Cup;
                        mirrored = slideSegment.slideType == SlideType.EdgeCurveCw;
                        break;

                    case SlideType.Fan:
                        shape = SlidePaths.PathShapes.Fan;
                        break;
                }

                endLane = locationToLane(slideSegment.vertices.Last());
                slideBodyParts.Add(new SlideBodyPart(shape,
                    (endLane - startLane).NormalizePath(),
                    mirrored
                ));
                startLane = endLane;
            }

            double millisPerBeat = controlPointInfo.TimingPointAt(note.parentCollection.time).BeatLength;
            slide.SlideInfoList.Add(new SlideBodyInfo
            {
                SlidePathParts = slideBodyParts.ToArray(),
                Duration = (path.delay + path.duration) * 1000f,
                Break = path.type == NoteType.Break,
                ShootDelay = (float)(path.delay * 1000f / millisPerBeat),
            });
        }
    }
}
