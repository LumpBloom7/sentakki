using System.IO;
using System.Linq;
using System.Text;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Sentakki.Beatmaps.Formats;
using osu.Game.Rulesets.Sentakki.Edit.Toolbox;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.SimaiUtils;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osuTK;
using SimaiSharp;

namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class SimaiEditorToolboxGroup : EditorToolboxGroup
{
    public SimaiEditorToolboxGroup()
        : base("Simai")
    {
    }

    [Resolved]
    private OverlayColourProvider colourProvider { get; set; } = null!;

    [Resolved]
    private Editor editor { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Children =
        [
            new ExpandableSpriteText()
            {
                Colour = colourProvider.Content2,
                ExpandedLabel = "Simai export is experimental. Use at your own risk."
            },
            new EditorToolButton("Export simai", () => new SpriteIcon { Icon = FontAwesome.Solid.FileExport }, () => new SimaiExportPopover()),
            new EditorToolButton("Import simai", () => new SpriteIcon { Icon = FontAwesome.Solid.FileImport }, () => new SimaiImportPopover())
        ];
    }

    private partial class SimaiImportPopover : OsuPopover
    {
        public SimaiImportPopover()
        {
            AllowableAnchors = [Anchor.CentreLeft, Anchor.CentreRight];
        }

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        private OsuFileSelector fileSelector = null!;
        private OsuButton button = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new GridContainer
            {
                Size = new Vector2(600, 400),
                RowDimensions =
                [
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 10),
                    new Dimension(GridSizeMode.Absolute, 45)
                ],
                Content = new Drawable[][]
                {
                    [
                        fileSelector = new OsuFileSelector(validFileExtensions: [".txt"])
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                    ],
                    [],
                    [
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children =
                            [
                                button = new RoundedButton
                                {
                                    Text = "Import",
                                    Width = 150f,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Action = importFromSimai
                                }
                            ]
                        }
                    ]
                }
            };

            fileSelector.CurrentFile.BindValueChanged(v => button.Enabled.Value = v.NewValue?.Extension == ".txt", true);
        }

        private void importFromSimai()
        {
            SimaiFile simaiFile = new SimaiFile(fileSelector.CurrentFile.Value);

            string? osuFile = SimaiOsz.OsuFromSimai(simaiFile).FirstOrDefault();

            if (osuFile is null)
            {
                notifications?.Post(new SimpleErrorNotification()
                {
                    Text = "Simai file doesn't contain any charts"
                });

                return;
            }

            var simaiBeatmapDecoder = new LegacySimaiBeatmapDecoder();
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(osuFile));
            using var lineBufferedReader = new IO.LineBufferedReader(memoryStream);

            var beatmap = simaiBeatmapDecoder.Decode(lineBufferedReader);
            var convertedBeatmap = (SentakkiBeatmap)new CompositeBeatmapConverter(beatmap, new SentakkiRuleset()).Convert();
            if (editorBeatmap.PlayableBeatmap is not SentakkiBeatmap senBeatmap)
                return;

            editorBeatmap.BeginChange();

            editorBeatmap.ControlPointInfo.Clear();

            foreach (var cp in convertedBeatmap.ControlPointInfo.AllControlPoints)
                editorBeatmap.ControlPointInfo.Add(cp.Time, cp);

            editorBeatmap.Clear();
            editorBeatmap.AddRange(convertedBeatmap.HitObjects);

            editorBeatmap.EndChange();

            notifications?.Post(new ProgressCompletionNotification()
            {
                Text = "Import successful"
            });
        }
    }

    private partial class SimaiExportPopover : OsuPopover
    {
        public SimaiExportPopover()
        {
            AllowableAnchors = [Anchor.CentreLeft, Anchor.CentreRight];
        }

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        [Resolved]
        private GameHost gameHost { get; set; } = null!;

        private OsuDirectorySelector directorySelector = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new GridContainer()
            {
                Size = new Vector2(600, 400),
                RowDimensions =
                [
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 10),
                    new Dimension(GridSizeMode.Absolute, 45)
                ],
                Content = new Drawable[][]
                {
                    [
                        directorySelector = new OsuDirectorySelector
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                    ],
                    [],
                    [
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children =
                            [
                                new RoundedButton
                                {
                                    Text = "Export",
                                    Width = 150f,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Action = exportToSimai
                                }
                            ]
                        }
                    ]
                }
            };
        }

        private void exportToSimai()
        {
            if (editorBeatmap.PlayableBeatmap is not IBeatmap<SentakkiHitObject> senBeatmap)
                return;

            var encoder = new QuantizedSimaiBeatmapEncoder(senBeatmap);

            var metadata = senBeatmap.BeatmapInfo.Metadata;
            string filename = SimaiOsz.CleanFileName($"(sen) {metadata.ArtistUnicode} - {metadata.TitleUnicode} ({senBeatmap.BeatmapInfo.DifficultyName}).txt");
            string path = $"{directorySelector.CurrentPath}/{filename}";
            var file = File.CreateText(path);

            try
            {
                encoder.Encode(file);
                notifications?.Post(new ProgressCompletionNotification()
                {
                    Text = "Encoding successful",
                    Activated = () => gameHost.PresentFileExternally(path)
                });
            }
            catch
            {
                notifications?.Post(new SimpleErrorNotification()
                {
                    Text = "Error encoding beatmap to simai"
                });
            }

            file.Close();
        }
    }
}
