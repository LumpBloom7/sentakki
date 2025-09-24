using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Difficulty;
using osu.Game.Rulesets.Sentakki.Edit;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Localisation;
using osu.Game.Rulesets.Sentakki.Mods;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Replays;
using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Rulesets.Sentakki.Statistics;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Edit.Setup;
using osu.Game.Screens.Ranking.Statistics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki;

public partial class SentakkiRuleset : Ruleset
{
    public override string RulesetAPIVersionSupported => CURRENT_RULESET_API_VERSION;

    private static readonly Lazy<bool> is_development_build
        = new Lazy<bool>(() => typeof(SentakkiRuleset).Assembly.GetCustomAttributes(false).OfType<DebuggableAttribute>().Any(da => da.IsJITTrackingEnabled));

    public static bool IsDevelopmentBuild => is_development_build.Value;

    public override string Description => IsDevelopmentBuild ? "sentakki (Dev build)" : "sentakki";
    public override string PlayingVerb => "Washing laundry";
    public override string ShortName => "Sentakki";

    public override ScoreProcessor CreateScoreProcessor() => new SentakkiScoreProcessor(this);
    public override HealthProcessor CreateHealthProcessor(double drainStartTime) => new SentakkiHealthProcessor();

    public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) =>
        new DrawableSentakkiRuleset(this, beatmap, mods);

    public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) =>
        new CompositeBeatmapConverter(beatmap, this);

    public override IBeatmapProcessor CreateBeatmapProcessor(IBeatmap beatmap) =>
        new SentakkiBeatmapProcessor(beatmap);

    public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) =>
        new SentakkiDifficultyCalculator(RulesetInfo, beatmap);

    public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new SentakkiReplayFrame();

    public override PerformanceCalculator CreatePerformanceCalculator() => new SentakkiPerformanceCalculator(this);

    public override IEnumerable<Mod> GetModsFor(ModType type)
    {
        switch (type)
        {
            case ModType.DifficultyReduction:
                return
                [
                    new MultiMod(new SentakkiModHalfTime(), new SentakkiModDaycore())
                ];

            case ModType.DifficultyIncrease:
                return
                [
                    new SentakkiModHardRock(),
                    new MultiMod(new SentakkiModSuddenDeath(), new SentakkiModPerfect()),
                    new MultiMod(new SentakkiModChallenge(), new SentakkiModAccuracyChallenge()),
                    new MultiMod(new SentakkiModDoubleTime(), new SentakkiModNightcore()),
                    new SentakkiModHidden()
                ];

            case ModType.Automation:
                return
                [
                    new SentakkiModAutoplay(),
                    new SentakkiModNoTouch()
                ];

            case ModType.Conversion:
                return
                [
                    new SentakkiModExperimental(),
                    new SentakkiModClassic(),
                    new SentakkiModMirror(),
                    new SentakkiModDifficultyAdjust()
                ];

            case ModType.Fun:
                return
                [
                    new MultiMod(new ModWindUp(), new ModWindDown()),
                    new SentakkiModBarrelRoll(),
                    new SentakkiModMuted(),
                    new ModAdaptiveSpeed(),
                    new SentakkiModSynesthesia()
                ];

            case ModType.System:
                return
                [
                    new SentakkiModTouchDevice()
                ];

            default:
                return [];
        }
    }

    public override RulesetSettingsSubsection CreateSettings() => new SentakkiSettingsSubsection(this);

    public override IRulesetConfigManager CreateConfig(SettingsStore? settings) => new SentakkiRulesetConfigManager(settings, RulesetInfo);

    public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) =>
    [
        new KeyBinding(InputKey.Z, SentakkiAction.Button1),
        new KeyBinding(InputKey.X, SentakkiAction.Button2),
        new KeyBinding(InputKey.MouseLeft, SentakkiAction.Button1),
        new KeyBinding(InputKey.MouseRight, SentakkiAction.Button2),
        new KeyBinding(InputKey.Number1, SentakkiAction.Key1),
        new KeyBinding(InputKey.Number2, SentakkiAction.Key2),
        new KeyBinding(InputKey.Number3, SentakkiAction.Key3),
        new KeyBinding(InputKey.Number4, SentakkiAction.Key4),
        new KeyBinding(InputKey.Number5, SentakkiAction.Key5),
        new KeyBinding(InputKey.Number6, SentakkiAction.Key6),
        new KeyBinding(InputKey.Number7, SentakkiAction.Key7),
        new KeyBinding(InputKey.Number8, SentakkiAction.Key8)
    ];

    public override StatisticItem[] CreateStatisticsForScore(ScoreInfo score, IBeatmap playableBeatmap) =>
    [
        new StatisticItem(SentakkiStatisticsStrings.TimingDistribution, () => new HitEventTimingDistributionGraph(score.HitEvents)
        {
            RelativeSizeAxes = Axes.X,
            Height = 250
        }, true),

        new StatisticItem(SentakkiStatisticsStrings.JudgementChart, () => new JudgementChart([.. score.HitEvents.Where(e => e.HitObject is SentakkiHitObject)])
        {
            RelativeSizeAxes = Axes.X,
            Size = new Vector2(1, 250)
        }, true),

        new StatisticItem(string.Empty, () => new SimpleStatisticTable(2, [
            new AverageHitError(score.HitEvents),
            new UnstableRate(score.HitEvents)
        ]), true)
    ];

    public override Drawable CreateIcon() =>
        new ConstrainedIconContainer
        {
            Icon = new SentakkiIcon(this),
            Size = new Vector2(100, 100)
        };

    protected override IEnumerable<HitResult> GetValidHitResults()
        => [HitResult.Perfect, HitResult.Great, HitResult.Good, HitResult.Ok];

    public override LocalisableString GetDisplayNameForHitResult(HitResult result) => result.GetDisplayNameForSentakkiResult();

    public override HitObjectComposer? CreateHitObjectComposer()
        => new SentakkiHitObjectComposer(this);

    public override IEnumerable<Drawable> CreateEditorSetupSections() =>
    [
        new MetadataSection(),
        new FillFlowContainer
        {
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(25f),
            Children =
            [
                new ResourcesSection { RelativeSizeAxes = Axes.X },
                new DesignSection { RelativeSizeAxes = Axes.X }
            ]
        },
    ];

    public override IBeatmapVerifier? CreateBeatmapVerifier()
        => new SentakkiBeatmapVerifier();

    public override bool EditorShowScrollSpeed => false;

    public partial class SentakkiIcon : CompositeDrawable
    {
        private readonly Ruleset ruleset;

        public SentakkiIcon(Ruleset ruleset)
        {
            Anchor = Origin = Anchor.Centre;
            this.ruleset = ruleset;
            FillAspectRatio = 1;
            FillMode = FillMode.Fit;
            Size = new Vector2(100, 100);
        }

        // We don't want to generate a new texture store everytime this used, so we create a single texture store for all usages of this icon.
        private static LargeTextureStore? textureStore;

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            textureStore ??= new LargeTextureStore(host.Renderer, host.CreateTextureLoaderStore(ruleset.CreateResourceStore()));

            AddInternal(new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Texture = textureStore.Get("Textures/SentakkiIcon.png"),
            });

            if (!IsDevelopmentBuild) return;

            AddInternal(new Container
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Size = new Vector2(60, 35),
                Children =
                [
                    // Used to offset the fonts being misaligned
                    new Container
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Size = new Vector2(60, 32),
                        CornerRadius = 8f,
                        CornerExponent = 2.5f,
                        Masking = true,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    },
                    new SpriteText
                    {
                        Text = "DEV",
                        Colour = Color4.Gray,
                        Font = OsuFont.Torus.With(size: 32, weight: FontWeight.Bold),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                ]
            });
        }
    }
}
