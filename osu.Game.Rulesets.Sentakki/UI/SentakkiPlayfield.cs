using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI;

[Cached]
public partial class SentakkiPlayfield : Playfield
{
    private readonly Container<HitExplosion> explosionLayer;

    private readonly JudgementContainer<DrawableSentakkiJudgement> judgementLayer;
    private readonly Container judgementAboveHitObjectLayer;

    private readonly JudgementPooler<DrawableSentakkiJudgement> newJudgementPool;
    private readonly DrawablePool<HitExplosion> explosionPool;

    private readonly SentakkiRing ring;

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

    public const float RINGSIZE = 600;
    public const float DOTSIZE = 20f;
    public const float INTERSECTDISTANCE = 296.5f;
    public const float NOTESTARTDISTANCE = 66f;

    public readonly LanedPlayfield LanedPlayfield;
    private readonly TouchPlayfield touchPlayfield;

    internal readonly Container AccentContainer;

    public static readonly float[] LANEANGLES =
    [
        22.5f,
        67.5f,
        112.5f,
        157.5f,
        202.5f,
        247.5f,
        292.5f,
        337.5f
    ];

    public SentakkiPlayfield()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.None;
        Rotation = 0;
        Size = new Vector2(RINGSIZE);
        AddRangeInternal([
            explosionPool = new DrawablePool<HitExplosion>(8),
            newJudgementPool = new JudgementPooler<DrawableSentakkiJudgement>([HitResult.Perfect, HitResult.Great, HitResult.Good, HitResult.Meh, HitResult.Miss], onJudgementLoaded),
            AccentContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children =
                [
                    new PlayfieldVisualisation(),
                        ring = new SentakkiRing()
                ]
            },
            explosionLayer = new Container<HitExplosion> { RelativeSizeAxes = Axes.Both },
            judgementLayer = new JudgementContainer<DrawableSentakkiJudgement>
            {
                RelativeSizeAxes = Axes.Both,
            },
            LanedPlayfield = new LanedPlayfield(),
            HitObjectContainer, // This only contains TouchHolds
            touchPlayfield = new TouchPlayfield(), // This only contains Touch notes, which needs to be above all other note types
            judgementAboveHitObjectLayer = new Container
            {
                RelativeSizeAxes = Axes.Both,
            },
        ]);
        AddNested(LanedPlayfield);
        AddNested(touchPlayfield);
        NewResult += onNewResult;
    }

    [Resolved]
    private DrawableSentakkiRuleset drawableSentakkiRuleset { get; set; } = null!;

    [Resolved]
    private SentakkiRulesetConfigManager? sentakkiRulesetConfig { get; set; }

    private Bindable<Skin> skin = null!;
    private readonly Bindable<ColorOption> ringColor = new Bindable<ColorOption>();

    private IBindable<StarDifficulty> beatmapDifficulty = null!;

    [BackgroundDependencyLoader]
    private void load(SkinManager skinManager, IBeatmap beatmap, BeatmapDifficultyCache difficultyCache)
    {
        RegisterPool<TouchHold, DrawableTouchHold>(2);
        RegisterPool<ScorePaddingObject, DrawableScorePaddingObject>(8);

        // handle colouring of playfield elements
        beatmapDifficulty = difficultyCache.GetBindableDifficulty(beatmap.BeatmapInfo);

        skin = skinManager.CurrentSkin.GetBoundCopy();
        sentakkiRulesetConfig?.BindWith(SentakkiRulesetSettings.RingColor, ringColor);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        skin.BindValueChanged(_ => changePlayfieldAccent(), true);
        ringColor.BindValueChanged(_ => changePlayfieldAccent(), true);
    }

    protected override HitObjectLifetimeEntry CreateLifetimeEntry(HitObject hitObject) => new SentakkiHitObjectLifetimeEntry(hitObject, drawableSentakkiRuleset);

    protected override GameplayCursorContainer CreateCursor() => new SentakkiCursorContainer();

    public override void Add(HitObject h)
    {
        switch (h)
        {
            case SentakkiLanedHitObject:
                LanedPlayfield.Add(h);
                break;

            case Touch:
                touchPlayfield.Add(h);
                break;

            default:
                base.Add(h);
                break;
        }
    }

    public override bool Remove(HitObject h)
    {
        switch (h)
        {
            case SentakkiLanedHitObject:
                return LanedPlayfield.Remove(h);

            case Touch:
                return touchPlayfield.Remove(h);

            default:
                return base.Remove(h);
        }
    }

    private void onJudgementLoaded(DrawableSentakkiJudgement drawableJudgement)
    {
        judgementAboveHitObjectLayer.Add(drawableJudgement.ProxiedAboveHitObjectsContent);
    }

    private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
    {
        if (!judgedObject.DisplayResult || !DisplayJudgements.Value || judgedObject is not DrawableSentakkiHitObject sentakkiHitObject)
            return;

        var judgement = newJudgementPool.Get(result.Type, d => d.Apply(result, judgedObject));
        if (judgement is not null)
            judgementLayer.Add(judgement);

        if (!result.IsHit) return;

        // Slide bodies don't need explosions, they just don't work.
        if (judgedObject is DrawableSlideBody)
            return;

        if (judgedObject.HitObject.Kiai)
            ring.KiaiBeat();

        var explosion = explosionPool.Get().Apply(sentakkiHitObject);
        explosionLayer.Add(explosion);
    }

    [Resolved]
    private OsuColour colours { get; set; } = null!;

    private void changePlayfieldAccent()
    {
        switch (ringColor.Value)
        {
            case ColorOption.Difficulty:
                double starRating = beatmapDifficulty.Value.Stars;
                var colour = colours.ForStarDifficulty(starRating);

                // Normalize the colors to make sure the ring is actually visible
                colour = Interpolation.ValueAt(0.5f, colour, new HSPAColour(colour) { P = 0.6f }.ToColor4(), 0, 1);

                AccentContainer.FadeColour(colour, 200);
                break;

            case ColorOption.Skin:
                AccentContainer.FadeColour(skin.Value.GetConfig<GlobalSkinColours, Color4>(GlobalSkinColours.MenuGlow)?.Value ?? Color4.White, 200);
                break;

            default:
                AccentContainer.FadeColour(Color4.White, 200);
                break;
        }
    }
}
