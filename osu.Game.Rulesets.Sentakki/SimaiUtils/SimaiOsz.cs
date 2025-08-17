using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using SimaiSharp;
using SimaiSharp.Structures;

namespace osu.Game.Rulesets.Sentakki.SimaiUtils;

public static class SimaiOsz
{
    private static readonly Dictionary<string, string> diff_index_to_dict = new Dictionary<string, string>
    {
        { "1", "EASY" },
        { "2", "BASIC" },
        { "3", "ADVANCED" },
        { "4", "EXPERT" },
        { "5", "MASTER" },
        { "6", "Re:MASTER" },
        { "7", "UTAGE" },
    };

    private const string osu_template = @"sentakki file format - simai v1
[General]
AudioFilename: {0}
AudioLeadIn: 0
PreviewTime: -1
Countdown: 0
SampleSet: Soft
StackLeniency: 1
Mode: 21
LetterboxInBreaks: 0
EditorBookmarks:

[Metadata]
Title:{1}
TitleUnicode:{2}
Artist:{3}
ArtistUnicode:{4}
Creator:{5}
Version:{6}
Source:{7}
Tags:{8}
MaiDiff:{9}
[Events]
{10}
[HitObjects]
{11}
";

    public static string CleanFileName(string fileName)
    {
        return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
    }

    public static IEnumerable<string> OsuFromSimai(SimaiFile file)
    {
        Dictionary<string, string> dict = file.ToKeyValuePairs().ToDictionary(x => x.Key, x => x.Value);

        string title = dict.GetValueOrDefault("title", "Unknown Title");
        string artist = dict.GetValueOrDefault("artist", "Unknown Artist");
        string allCreator = dict.GetValueOrDefault("des", "-");
        string first = dict.GetValueOrDefault("first", "0");

        foreach (var (k, v) in dict)
        {
            if (!k.StartsWith("inote", StringComparison.Ordinal))
            {
                continue;
            }

            string diffIndex = k.Split("_")[1];
            string diffName = diff_index_to_dict.GetValueOrDefault(diffIndex, $"Extra - {diffIndex}");

            if (v.Length == 0) continue;

            string chart = string.Join('\n', v.Split('\n').Select(x => x.TrimStart()));

            if (first != "0")
            {
                chart = $"{{#{first}}}," + chart;
            }

            MaiChart maiChart = SimaiConvert.Deserialize(chart);

            bool isDeluxe = maiChart.NoteCollections.Any(noteCollection => noteCollection.Any(note =>
                    note.IsEx // Ex note
                    || note.slidePaths.Any(slidePath => slidePath.segments.Count > 1 || slidePath.type == NoteType.Break) // Chain slide | Break slide
                    || note.location.group != NoteGroup.Tap // TOUCH
            ));

            string creator = dict.GetValueOrDefault($"des_{diffIndex}", allCreator);
            string level = dict.GetValueOrDefault($"lv_{diffIndex}", "0");
            string version = dict.GetValueOrDefault($"version", "maimai");
            string genre = dict.GetValueOrDefault("genre", "");

            string[] tags = { "sentakki-legacy", level, creator, isDeluxe ? "deluxe" : "standard", genre };

            string osuFile = string.Format(
                osu_template,
                "",
                title,
                title,
                artist,
                artist,
                creator,
                diffName,
                version,
                string.Join(" ", tags),
                level,
                "",
                chart);
            yield return osuFile;
        }
    }

    public static void ConvertToOsz(DirectoryInfo path, Func<string, Stream> createOutputStream, bool closeStream = true)
    {
        SimaiFile simaiFile = new SimaiFile(path.EnumerateFileSystemInfos().First(f => f.Name is "maidata.txt"));
        Dictionary<string, string> dict = simaiFile.ToKeyValuePairs().ToDictionary(x => x.Key, x => x.Value);

        string title = dict.GetValueOrDefault("title", "Unknown Title");
        string artist = dict.GetValueOrDefault("artist", "Unknown Artist");
        string allCreator = dict.GetValueOrDefault("des", "-");
        string first = dict.GetValueOrDefault("first", "0");

        string trackName = "track.mp3";

        string filename = $"{CleanFileName($"{artist} - {title}")}].osz";
        Stream zipStream = createOutputStream(filename);

        var events = new List<string>();

        using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            foreach (FileInfo file in path.EnumerateFiles())
            {
                bool copy = false;

                if (file.Name is "track.mp3" or "track.ogg")
                {
                    trackName = file.Name;
                    copy = true;
                }

                if (file.Name is "pv.mp4" or "bga.mp4")
                {
                    events.Add($"1,0,{file.Name}");
                    copy = true;
                }
                else if (file.Name is "bg.jpg" or "bg.jpeg" or "bg.png")
                {
                    events.Add($"0,0,{file.Name}");
                    copy = true;
                }

                if (copy)
                {
                    ZipArchiveEntry entry = zip.CreateEntry(file.Name, CompressionLevel.Fastest);
                    using Stream entryStream = entry.Open();
                    using FileStream fileStream = file.OpenRead();
                    fileStream.CopyTo(entryStream);
                }
            }

            foreach (var (k, v) in dict)
            {
                if (!k.StartsWith("inote", StringComparison.Ordinal))
                {
                    continue;
                }

                string diffIndex = k.Split("_")[1];
                string diffName = diff_index_to_dict.GetValueOrDefault(diffIndex, $"Extra - {diffIndex}");

                if (v.Length == 0) continue;

                string chart = string.Join('\n', v.Split('\n').Select(x => x.TrimStart()));

                if (first != "0")
                {
                    chart = $"{{#{first}}}," + chart;
                }

                MaiChart maiChart = SimaiConvert.Deserialize(chart);

                bool isDeluxe = maiChart.NoteCollections.Any(noteCollection => noteCollection.Any(note =>
                        note.IsEx // Ex note
                        || note.slidePaths.Any(slidePath => slidePath.segments.Count > 1 || slidePath.type == NoteType.Break) // Chain slide | Break slide
                        || note.location.group != NoteGroup.Tap // TOUCH
                ));

                string creator = dict.GetValueOrDefault($"des_{diffIndex}", allCreator);
                string level = dict.GetValueOrDefault($"lv_{diffIndex}", "0");
                string version = dict.GetValueOrDefault($"version", "maimai");
                string genre = dict.GetValueOrDefault("genre", "");

                string[] tags = { "sentakki-legacy", level, creator, isDeluxe ? "deluxe" : "standard", genre };

                string osuFile = string.Format(
                    osu_template,
                    trackName,
                    title,
                    title,
                    artist,
                    artist,
                    creator,
                    diffName,
                    version,
                    string.Join(" ", tags),
                    level,
                    string.Join("\n", events),
                    chart);
                ZipArchiveEntry entry = zip.CreateEntry($"{diffName}.osu", CompressionLevel.Fastest);
                using Stream entryStream = entry.Open();
                entryStream.Write(System.Text.Encoding.UTF8.GetBytes(osuFile));
            }
        }

        if (closeStream)
            zipStream.Close();
    }
}
