using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Sentakki.SimaiUtils;
using osu.Game.Screens;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI;

#nullable disable

public partial class SentakkiSimaiConvertScreen : OsuScreen
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
    private OsuColour colours { get; set; }

    [Resolved]
    private GameHost host { get; set; }

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
                            Text = "Convert recursively from subfolders",
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
                            Text = "Convert",
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = button_height,
                            Width = 0.9f,
                            Margin = new MarginPadding { Vertical = button_vertical_margin },
                            Action = () => startConvertTask(fileSelector.CurrentPath.Value)
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

        if (directories.Count == 0)
            return;

        startConvertTask(directories, path);
    }


    private void startConvertTask(DirectoryInfo path)
    {
        Task.Factory.StartNew(
            () =>
            {
                string oszPath = path.FullName;

                FileStream createOutputStream(string filename)
                {
                    oszPath = $"{path.Parent.FullName}{Path.DirectorySeparatorChar}{filename}";
                    return File.Open(oszPath, FileMode.Create);
                }

                try
                {
                    SimaiOsz.ConvertToOsz(path, createOutputStream);
                }
                catch (Exception e)
                {
                    notificationOverlay.Post(new SimpleErrorNotification
                    {
                        Text = $"Failed to convert {path.Name}: {e.Message}"
                    });
                    return;
                }

                notificationOverlay.Post(new SimpleNotification
                {
                    Text = $"Successfully converted {path.Name}. Click to view converted file.",
                    Activated = () => host.PresentFileExternally(oszPath)
                });
            }, TaskCreationOptions.LongRunning);
    }

    private void startConvertTask(List<DirectoryInfo> paths, DirectoryInfo origin)
    {
        var notification = new ProgressNotification
        {
            Progress = 0,
            State = ProgressNotificationState.Active,
            Text = "Converting simai files...",
            CompletionText = $"Converted {paths.Count} simai beatmaps. Click to view converted files.",
        };

        notificationOverlay.Post(notification);

        string batchOutputFolder = $"{origin.FullName}{Path.DirectorySeparatorChar}osz";

        Task.Factory.StartNew(() =>
        {
            foreach (var (index, item) in paths.Select((i, v) => (v, i)))
            {
                FileStream createOutputStream(string filename)
                {
                    string oszDir = $"{item.Parent.FullName.Replace(origin.FullName, batchOutputFolder)}{Path.DirectorySeparatorChar}";
                    Directory.CreateDirectory(oszDir);
                    string oszPath = $"{oszDir}{filename}";
                    return File.Open(oszPath, FileMode.Create);
                }

                try
                {
                    SimaiOsz.ConvertToOsz(item, createOutputStream);
                }
                catch (Exception e)
                {
                    notificationOverlay.Post(new SimpleErrorNotification
                    {
                        Text = $"Failed to convert {item.Name}: {e.Message}"
                    });
                }

                notification.Text = $"Converted {index + 1}/{paths.Count} simai beatmaps.";
                notification.Progress = (float)(index + 1) / paths.Count;

            }
            notification.CompletionClickAction = () => host.OpenFileExternally(batchOutputFolder);
            notification.State = ProgressNotificationState.Completed;
        }, TaskCreationOptions.LongRunning);
    }
}
