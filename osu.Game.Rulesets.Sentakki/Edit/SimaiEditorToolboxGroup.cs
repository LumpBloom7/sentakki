using System.IO;
using System.Linq;
using System.Text;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Sentakki.Beatmaps.Formats;
using osu.Game.Rulesets.Sentakki.Edit.Toolbox;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.SimaiUtils;
using osu.Game.Screens;
using osu.Game.Screens.Edit;
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
            new ExpandableButton()
            {
                ContractedLabelText = "Export as simai",
                ExpandedLabelText = "Export as simai",
                RelativeSizeAxes = Axes.X,
                Action = () => editor.Push(new ExportDialog(editor.Beatmap.Value))
            },
            new ExpandableButton()
            {
                ContractedLabelText = "Import simai chart",
                ExpandedLabelText = "Import simai chart",
                RelativeSizeAxes = Axes.X,
                Action = () => editor.Push(new ImportDialog(editor.Beatmap.Value))
            }
        ];
    }

    private abstract partial class SimaiDialogBase : OsuScreen
    {
        public override bool HideOverlaysOnEnter => true;
        public override bool ShowFooter => true;

        [Resolved]
        private Editor editor { get; set; } = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        protected EditorBeatmap EditorBeatmap => editorBeatmap;

        protected WorkingBeatmap CurrentWorkingBeatmap { get; private set; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        protected OverlayColourProvider ColourProvider => colourProvider;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        protected INotificationOverlay? Notifications => notifications;

        [Resolved]
        private GameHost gameHost { get; set; } = null!;

        protected GameHost GameHost => gameHost;

        public SimaiDialogBase(WorkingBeatmap workingBeatmap)
        {
            CurrentWorkingBeatmap = workingBeatmap;
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            // Let's manually restore this, otherwise the user will return to a fucked up state
            editor.Beatmap.Value = CurrentWorkingBeatmap;
            return base.OnExiting(e);
        }
    }

    private partial class ExportDialog : SimaiDialogBase
    {
        public ExportDialog(WorkingBeatmap workingBeatmap)
            : base(workingBeatmap)
        {
        }

        private OsuDirectorySelector folderSelector = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new Container
            {
                Masking = true,
                CornerRadius = 10,
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(0.9f, 0.8f),
                Children =
                [
                    new Box
                    {
                        Colour = ColourProvider.Background4,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions =
                        [
                            new Dimension(GridSizeMode.Distributed),
                            new Dimension(GridSizeMode.Absolute, 45)
                        ],
                        Content = new Drawable[][]
                        {
                            [
                                folderSelector = new OsuDirectorySelector()
                                {
                                    RelativeSizeAxes = Axes.Both
                                },
                            ],
                            [
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding
                                    {
                                        Horizontal = 20,
                                        Vertical = 10
                                    },
                                    Children =
                                    [
                                        new RoundedButton
                                        {
                                            Text = "Export here",
                                            Width = 150f,
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            Action = exportToSimai
                                        }
                                    ]
                                }
                            ]
                        }
                    }
                ]
            };
        }

        private void exportToSimai()
        {
            if (EditorBeatmap.PlayableBeatmap is not IBeatmap<SentakkiHitObject> senBeatmap)
                return;

            var encoder = new QuantizedSimaiBeatmapEncoder(senBeatmap);

            var metadata = senBeatmap.BeatmapInfo.Metadata;
            string filename = SimaiOsz.CleanFileName($"(sen) {metadata.ArtistUnicode} - {metadata.TitleUnicode} ({senBeatmap.BeatmapInfo.DifficultyName}).txt");
            string path = $"{folderSelector.CurrentPath}{Path.DirectorySeparatorChar}{filename}";
            var file = File.CreateText(path);

            try
            {
                encoder.Encode(file);
                Notifications?.Post(new ProgressCompletionNotification()
                {
                    Text = "Encoding successful",
                    Activated = () => GameHost.OpenFileExternally(path)
                });
            }
            catch
            {
                Notifications?.Post(new SimpleErrorNotification()
                {
                    Text = "Error encoding beatmap to simai"
                });
            }

            file.Close();
        }
    }

    private partial class ImportDialog : SimaiDialogBase
    {
        public ImportDialog(WorkingBeatmap workingBeatmap)
            : base(workingBeatmap)
        {
        }

        private OsuFileSelector fileSelector = null!;

        private RoundedButton button = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new Container
            {
                Masking = true,
                CornerRadius = 10,
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(0.9f, 0.8f),
                Children =
                [
                    new Box
                    {
                        Colour = ColourProvider.Background4,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions =
                        [
                            new Dimension(GridSizeMode.Distributed),
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
                            [
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding
                                    {
                                        Horizontal = 20,
                                        Vertical = 10
                                    },
                                    Children =
                                    [
                                        button = new RoundedButton
                                        {
                                            Text = "Import file",
                                            Width = 150f,
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            Action = importFromSimai
                                        }
                                    ]
                                }
                            ]
                        }
                    }
                ]
            };

            fileSelector.CurrentFile.BindValueChanged(v => button.Enabled.Value = v.NewValue?.Extension == ".txt", true);
        }

        private void importFromSimai()
        {
            SimaiFile simaiFile = new SimaiFile(fileSelector.CurrentFile.Value);

            string? osuFile = SimaiOsz.OsuFromSimai(simaiFile).FirstOrDefault();

            if (osuFile is null)
            {
                Notifications?.Post(new SimpleErrorNotification()
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
            if (EditorBeatmap.PlayableBeatmap is not SentakkiBeatmap senBeatmap)
                return;

            EditorBeatmap.ControlPointInfo.Clear();

            foreach (var cp in convertedBeatmap.ControlPointInfo.AllControlPoints)
                EditorBeatmap.ControlPointInfo.Add(cp.Time, cp);

            EditorBeatmap.Clear();
            EditorBeatmap.AddRange(convertedBeatmap.HitObjects);

            var encoder = new QuantizedSimaiBeatmapEncoder(senBeatmap);
            Notifications?.Post(new ProgressCompletionNotification()
            {
                Text = "Import successful"
            });
        }
    }
}
