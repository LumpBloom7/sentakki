﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens;
using osuTK;
using SimaiSharp;
using SimaiSharp.Structures;

namespace osu.Game.Rulesets.Sentakki.UI;

#nullable disable

public partial class SentakkiSimaiImportScreen : OsuScreen
{
    public override bool HideOverlaysOnEnter => true;

    private OsuFileSelector fileSelector;
    private Container contentContainer;
    private INotificationOverlay notificationOverlay;

    private RoundedButton importRecursiveButton;
    private RoundedButton importButton;

    private const float duration = 300;
    private const float button_height = 50;
    private const float button_vertical_margin = 15;

    [Resolved]
    private OsuGameBase game { get; set; }

    [Resolved]
    private OsuColour colours { get; set; }

    [Cached]
    private OverlayColourProvider overlayColourProvider { get; set; } = new OverlayColourProvider(OverlayColourScheme.Pink);

    [BackgroundDependencyLoader(true)]
    private void load(INotificationOverlay notificationOverlay)
    {
        this.notificationOverlay = notificationOverlay;
        InternalChild = contentContainer = new Container
        {
            Masking = true,
            CornerRadius = 10,
            RelativeSizeAxes = Axes.Both,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Size = new Vector2(0.9f, 0.8f),
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = colours.GreySeaFoamDark,
                    RelativeSizeAxes = Axes.Both,
                },
                fileSelector = new OsuFileSelector(validFileExtensions: Array.Empty<string>())
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.65f
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.35f,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colours.GreySeaFoamDarker,
                            RelativeSizeAxes = Axes.Both
                        },
                        importRecursiveButton = new RoundedButton
                        {
                            Text = "Import recursively from subfolders (JANKY)",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.X,
                            Height = button_height,
                            Width = 0.9f,
                            Margin = new MarginPadding { Vertical = button_vertical_margin },
                            Action = () => startRecursiveBatch(fileSelector.CurrentPath.Value)
                        },
                        importButton = new RoundedButton
                        {
                            Text = "Import",
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = button_height,
                            Width = 0.9f,
                            Margin = new MarginPadding { Vertical = button_vertical_margin },
                            Action = () => startImportTask(fileSelector.CurrentPath.Value)
                        },
                    }
                }
            }
        };
        fileSelector.CurrentPath.BindValueChanged(directoryChanged);
    }

    public override void OnEntering(ScreenTransitionEvent e)
    {
        base.OnEntering(e);

        contentContainer.ScaleTo(0.95f).ScaleTo(1, duration, Easing.OutQuint);
        this.FadeInFromZero(duration);
    }

    public override bool OnExiting(ScreenExitEvent e)
    {
        contentContainer.ScaleTo(0.95f, duration, Easing.OutQuint);
        this.FadeOut(duration, Easing.OutQuint);

        return base.OnExiting(e);
    }

    private void directoryChanged(ValueChangedEvent<DirectoryInfo> e)
    {
        if (e.NewValue == null)
        {
            importRecursiveButton.Enabled.Value = false;
            importButton.Enabled.Value = false;
            return;
        }

        importRecursiveButton.Enabled.Value = e.NewValue.EnumerateDirectories().Any();
        importButton.Enabled.Value = e.NewValue.EnumerateFiles().Any(f => f.Name is "maidata.txt");
        fileSelector.CurrentFile.Value = null;
    }

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
Artist:{2}
ArtistUnicode:{2}
Creator:{3}
Version:{4}
Source:maimai
Tags:{5}
[Events]
{6}
[HitObjects]
{7}
";

    private void startRecursiveBatch(DirectoryInfo path)
    {
        static void recurse(DirectoryInfo path, List<DirectoryInfo> directories)
        {
            if (path.EnumerateFiles().Any(f => f.Name is "maidata.txt"))
            {
                directories.Add(path);
            }

            foreach (DirectoryInfo directoryInfo in path.EnumerateDirectories())
            {
                recurse(directoryInfo, directories);
            }
        }

        List<DirectoryInfo> directories = new List<DirectoryInfo>();

        foreach (DirectoryInfo directoryInfo in path.EnumerateDirectories())
        {
            recurse(directoryInfo, directories);
        }

        startImportTask(directories);
    }

    private void startImportTask(DirectoryInfo path)
    {
        Task.Factory.StartNew(async () =>
            {
                await game.Import(new[] { startImport(path) }).ConfigureAwait(false);
            }
            , TaskCreationOptions.LongRunning);
    }

    private void startImportTask(List<DirectoryInfo> paths)
    {
        if (paths.Count == 0)
        {
            return;
        }

        var notification = new ProgressNotification
        {
            Progress = 0,
            State = ProgressNotificationState.Active,
            Text = "Importing simai files...",
            CompletionText = $"Imported {paths.Count} simai beatmaps",
        };

        notificationOverlay.Post(notification);

        Task.Factory.StartNew(async () =>
        {
            foreach (var (index, item) in paths.Select((i, v) => (v, i)))
            {
                try
                {
                    // TODO: Figure out how to avoid creating too many "import initialization" notifications
                    await game.Import(new[] { startImport(item) }).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    notificationOverlay.Post(new SimpleErrorNotification
                    {
                        Text = $"Failed to import {item.Name}: {e.Message}"
                    });
                }

                notification.Text = $"Imported {index + 1}/{paths.Count} simai beatmaps";
                notification.Progress = (float)(index + 1) / paths.Count;
            }

            notification.State = ProgressNotificationState.Completed;
        }, TaskCreationOptions.LongRunning);
    }

    private ImportTask startImport(DirectoryInfo path)
    {
        SimaiFile simaiFile = new SimaiFile(path.EnumerateFileSystemInfos().First(f => f.Name is "maidata.txt"));
        Dictionary<string, string> dict = simaiFile.ToKeyValuePairs().ToDictionary(x => x.Key, x => x.Value);

        string title = dict.GetValueOrDefault("title", "Unknown Title");
        string artist = dict.GetValueOrDefault("artist", "Unknown Artist");
        string allCreator = dict.GetValueOrDefault("des", "-");
        string first = dict.GetValueOrDefault("first", "0");

        string trackName = "track.mp3";

        MemoryStream zipStream = new MemoryStream();
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

                string[] tags = { "sentakki-legacy", level, creator, isDeluxe ? "deluxe" : "standard" };

                string osuFile = string.Format(osu_template, trackName, title, artist, creator, diffName, string.Join(" ", tags), string.Join("\n", events), chart);
                ZipArchiveEntry entry = zip.CreateEntry($"{diffName}.osu", CompressionLevel.Fastest);
                using Stream entryStream = entry.Open();
                entryStream.Write(System.Text.Encoding.UTF8.GetBytes(osuFile));
            }
        }

        return new ImportTask(zipStream, $"{artist} - {title}.osz");
    }
}
