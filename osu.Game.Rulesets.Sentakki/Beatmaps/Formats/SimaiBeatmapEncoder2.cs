using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using SimaiSharp;
using SimaiSharp.Internal.SyntacticAnalysis;
using SimaiSharp.Structures;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Formats;

public class SimaiBeatmap
{
    internal static Dictionary<Vector2, Location> PositionsToLocations = SentakkiPlayfield
        .LANEANGLES.SelectMany<float, KeyValuePair<Vector2, Location>>(
            (angle, index) =>

                [
                    new(
                        SentakkiExtensions.GetCircularPosition(130, angle),
                        new(index, NoteGroup.BSensor)
                    ),
                    new(
                        SentakkiExtensions.GetCircularPosition(190, angle - 22.5f),
                        new(index, NoteGroup.ESensor)
                    ),
                    new(
                        SentakkiExtensions.GetCircularPosition(270, angle),
                        new(index, NoteGroup.ASensor)
                    ),
                    new(
                        SentakkiExtensions.GetCircularPosition(270, angle - 22.5f),
                        new(index, NoteGroup.DSensor)
                    ),
                ]
        )
        .Append(new(new(0, 0), new(0, NoteGroup.CSensor)))
        .ToDictionary();

    private MaiChart maiChart = new();
    private Beatmap<SentakkiHitObject> beatmap;

    public SimaiBeatmap(Beatmap<SentakkiHitObject> beatmap)
    {
        this.beatmap = beatmap;
        // Encode ending
        maiChart.FinishTiming = (float)(beatmap.BeatmapInfo.Length / 1000.0);
        List<TimingChange> timingChanges = [];

        if (beatmap.ControlPointInfo.TimingPoints.Count == 0)
            return;

        // Add a virtual padding timingPoint to accomodate simaiSharp devs being braindead
        double timeBeforeFirstTimingPoint = beatmap.ControlPointInfo.TimingPoints[0].Time / 1000;
        if (timeBeforeFirstTimingPoint > 0)
        {
            TimingChange timingChange = new TimingChange
            {
                subdivisions = 4,
                tempo = (float)(60 / timeBeforeFirstTimingPoint),
                time = 0
            };

            timingChanges.Add(timingChange);
        }

        foreach (var timingPoint in beatmap.ControlPointInfo.TimingPoints)
        {
            double coercedTime = timingPoint.Time / 1000;
            TimingChange timingChange = new TimingChange
            {
                subdivisions = 128,
                tempo = 60 / (float)(timingPoint.BeatLength / 1000),
                time = (float)coercedTime,
            };

            Console.WriteLine($"{timingChange.SecondsPerBar}, {(float)(timingPoint.BeatLength / 1000)}");
            timingChanges.Add(timingChange);
        }

        List<NoteCollection> noteCollections = [];

        foreach (var hitObjectGroup in beatmap.HitObjects.GroupBy(h => h.StartTime))
        {
            NoteCollection noteCollection = new NoteCollection((float)(hitObjectGroup.Key / 1000));

            foreach (var hitObject in hitObjectGroup)
                noteCollection.Add(hitObjectToNote(hitObject, noteCollection));

            noteCollections.Add(noteCollection);
        }

        maiChart = new MaiChart
        {
            FinishTiming = (float)(beatmap.BeatmapInfo.Length / 1000),
            NoteCollections = [.. noteCollections],
            TimingChanges = [.. timingChanges]
        };
    }

    private Note hitObjectToNote(SentakkiHitObject hitObject, NoteCollection parentCollection)
    {
        Note note = new Note(parentCollection);

        switch (hitObject)
        {
            case Tap tap:
                note.type = tap.Break ? NoteType.Break : NoteType.Tap;
                note.location = new Location(tap.Lane, NoteGroup.Tap);
                note.styles |= tap.Ex ? NoteStyles.Ex : 0;
                break;
            case Hold hold:
                note.type = hold.Break ? NoteType.Break : NoteType.Hold;
                note.location = new Location(hold.Lane, NoteGroup.Tap);
                note.length = (float)(hold.Duration / 1000);
                note.styles |= hold.Ex ? NoteStyles.Ex : 0;
                break;
            case Touch touch:
                note.location = PositionsToLocations.MinBy(kv =>
                    {
                        double xDelta = Math.Abs(kv.Key.X - hitObject.Position.X);
                        double yDelta = Math.Abs(kv.Key.Y - hitObject.Position.Y);
                        return Math.Sqrt(xDelta * xDelta + yDelta * yDelta);
                    }).Value;
                note.type = NoteType.Touch;
                break;
            case TouchHold touchHold:
                note.location = new Location(0, NoteGroup.CSensor);
                note.length = (float)(touchHold.Duration / 1000);
                break;
            case Slide slide:
                note.type = slide.Break ? NoteType.Break : NoteType.Tap;
                note.location = new Location(slide.Lane, NoteGroup.Tap);
                note.styles |= slide.Ex ? NoteStyles.Ex : 0;

                if (slide.TapType == Slide.TapTypeEnum.None)
                    note.type = NoteType.ForceInvalidate;

                if (slide.TapType == Slide.TapTypeEnum.Tap)
                    note.appearance = NoteAppearance.ForceNormal;

                if (slide.SlideBodies.Count == 0)
                {
                    note.appearance = NoteAppearance.ForceStar;
                    break;
                }

                List<SlidePath> slidePaths = [];

                foreach (var slideBodyInfo in slide.SlideInfoList)
                {
                    List<SlideSegment> segments = [];
                    int currentLane = slide.Lane;

                    foreach (var part in slideBodyInfo.SlidePathParts)
                    {
                        SlideSegment segment = new();
                        currentLane = (currentLane + part.EndOffset).NormalizePath();
                        segment.vertices = [new(currentLane, NoteGroup.Tap)];
                        segment.slideType = part.Shape switch
                        {
                            SlidePaths.PathShapes.Fan => SlideType.Fan,
                            SlidePaths.PathShapes.Circle => part.Mirrored ? SlideType.RingCcw : SlideType.RingCw,
                            SlidePaths.PathShapes.V => SlideType.Fold,
                            SlidePaths.PathShapes.U => part.Mirrored ? SlideType.CurveCw : SlideType.CurveCcw,
                            SlidePaths.PathShapes.Thunder => part.Mirrored ? SlideType.ZigZagZ : SlideType.ZigZagS,
                            SlidePaths.PathShapes.Cup => part.Mirrored ? SlideType.EdgeCurveCw : SlideType.EdgeCurveCcw,
                            SlidePaths.PathShapes.Straight => SlideType.StraightLine,
                            _ => SlideType.StraightLine,
                        };
                        segments.Add(segment);
                    }
                    double millisPerBeat = beatmap.ControlPointInfo.TimingPointAt(hitObject.StartTime).BeatLength;

                    double shootDelayMs = slideBodyInfo.ShootDelay * millisPerBeat;

                    SlidePath path = new(segments)
                    {
                        duration = (float)((slideBodyInfo.Duration - shootDelayMs) / 1000),
                        type = slideBodyInfo.Break ? NoteType.Break : NoteType.Slide,
                        delay = (float)(shootDelayMs / 1000)
                    };
                    slidePaths.Add(path);
                }
                note.slidePaths = slidePaths;
                break;
        }

        return note;
    }

    public void SerializeToFile()
    {
        var metadata = beatmap.BeatmapInfo.Metadata;
        string path = $"(sen) {metadata.ArtistUnicode} - {metadata.TitleUnicode} ({beatmap.BeatmapInfo.DifficultyName}).txt";
        StringBuilder fileContentBuilder = new();
        fileContentBuilder.AppendLine($"&title={metadata.TitleUnicode}");
        fileContentBuilder.AppendLine($"&artist={metadata.ArtistUnicode}");
        fileContentBuilder.AppendLine($"&wholebpm={beatmap.BeatmapInfo.BPM}");
        fileContentBuilder.AppendLine($"&lv_7=洗");
        fileContentBuilder.AppendLine($"&inote_7={SimaiConvert.Serialize(maiChart)}");

        var file = File.CreateText(path);
        file.Write(fileContentBuilder);
        file.Close();
    }
}
