using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Skinning;
using osu.Game.Rulesets.Sentakki.Skinning.Default;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI;

[Cached]
public partial class SentakkiPlayfield : Playfield
{
    private readonly Container<DrawableSentakkiJudgement> judgementLayer;

    private readonly Container<HitExplosion> explosionLayer;

    private readonly DrawablePool<DrawableSentakkiJudgement> judgementPool;
    private readonly DrawablePool<HitExplosion> explosionPool;

    private readonly SkinnableDrawable ring;

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
            judgementPool = new DrawablePool<DrawableSentakkiJudgement>(8),
            AccentContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children =
                [
                    ring = new SkinnableDrawable(new SentakkiSkinComponentLookup(SentakkiSkinComponents.PlayfieldRing), _ => new PlayfieldRing())
                ]
            },
            explosionLayer = new Container<HitExplosion> { RelativeSizeAxes = Axes.Both },
            LanedPlayfield = new LanedPlayfield(),
            HitObjectContainer, // This only contains TouchHolds
            touchPlayfield = new TouchPlayfield(), // This only contains Touch notes, which needs to be above all other note types
            judgementLayer = new Container<DrawableSentakkiJudgement>
            {
                RelativeSizeAxes = Axes.Both,
            }
        ]);
        AddNested(LanedPlayfield);
        AddNested(touchPlayfield);
        NewResult += onNewResult;
    }

    [Resolved]
    private DrawableSentakkiRuleset drawableSentakkiRuleset { get; set; } = null!;

    private Bindable<Skin> skin = null!;

    [BackgroundDependencyLoader]
    private void load(SkinManager skinManager, IBeatmap beatmap, BeatmapDifficultyCache difficultyCache)
    {
        RegisterPool<TouchHold, DrawableTouchHold>(2);
        RegisterPool<ScorePaddingObject, DrawableScorePaddingObject>(8);

        skin = skinManager.CurrentSkin.GetBoundCopy();
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

    private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
    {
        if (!judgedObject.DisplayResult || !DisplayJudgements.Value || judgedObject is not DrawableSentakkiHitObject sentakkiHitObject)
            return;

        if (!(skin.Value is ArgonProSkin && result.Type >= HitResult.Great))
            judgementLayer.Add(judgementPool.Get().Apply(result, judgedObject));

        if (!result.IsHit) return;

        // Slide bodies don't need explosions, they just don't work.
        if (judgedObject is DrawableSlideBody)
            return;

        var explosion = explosionPool.Get().Apply(sentakkiHitObject);
        explosionLayer.Add(explosion);
    }


}
