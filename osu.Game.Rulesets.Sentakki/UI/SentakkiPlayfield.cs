using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI
{
    [Cached]
    public class SentakkiPlayfield : Playfield
    {
        private readonly Container<DrawableSentakkiJudgement> judgementLayer;
        private readonly DrawablePool<DrawableSentakkiJudgement> judgementPool;

        public readonly SentakkiRing Ring;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public const float RINGSIZE = 600;
        public const float DOTSIZE = 20f;
        public const float INTERSECTDISTANCE = 296.5f;
        public const float NOTESTARTDISTANCE = 66f;

        private readonly LanedPlayfield lanedPlayfield;
        private readonly TouchPlayfield touchPlayfield;

        public static readonly float[] LANEANGLES =
        {
            22.5f,
            67.5f,
            112.5f,
            157.5f,
            202.5f,
            247.5f,
            292.5f,
            337.5f
        };

        public SentakkiPlayfield()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.None;
            Rotation = 0;
            Size = new Vector2(600);
            AddRangeInternal(new Drawable[]
            {
                judgementPool = new DrawablePool<DrawableSentakkiJudgement>(8),
                new PlayfieldVisualisation(),
                Ring = new SentakkiRing(),
                lanedPlayfield = new LanedPlayfield(),
                HitObjectContainer, // This only contains TouchHolds, which needs to be above others types
                touchPlayfield = new TouchPlayfield(), // This only contains Touch, which needs a custom playfield to handle their input
                judgementLayer = new Container<DrawableSentakkiJudgement>
                {
                    RelativeSizeAxes = Axes.Both,
                }
            });
            AddNested(lanedPlayfield);
            AddNested(touchPlayfield);
            NewResult += onNewResult;
        }

        private DrawableSentakkiRuleset drawableSentakkiRuleset;
        private SentakkiRulesetConfigManager sentakkiRulesetConfig;

        [BackgroundDependencyLoader(true)]
        private void load(DrawableSentakkiRuleset drawableRuleset, SentakkiRulesetConfigManager sentakkiRulesetConfigManager)
        {
            drawableSentakkiRuleset = drawableRuleset;
            sentakkiRulesetConfig = sentakkiRulesetConfigManager;

            RegisterPool<TouchHold, DrawableTouchHold>(2);
        }

        protected override HitObjectLifetimeEntry CreateLifetimeEntry(HitObject hitObject) => new SentakkiHitObjectLifetimeEntry(hitObject, sentakkiRulesetConfig, drawableSentakkiRuleset);

        protected override GameplayCursorContainer CreateCursor() => new SentakkiCursorContainer();

        public override void Add(HitObject h)
        {
            switch (h)
            {
                case SentakkiLanedHitObject _:
                    lanedPlayfield.Add(h);
                    break;
                case Objects.Touch _:
                    touchPlayfield.Add(h);
                    break;
                default:
                    base.Add(h);
                    break;
            }
        }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (!judgedObject.DisplayResult || !DisplayJudgements.Value || !(judgedObject is DrawableSentakkiHitObject))
                return;

            judgementLayer.Add(judgementPool.Get(j => j.Apply(result, judgedObject)));

            if (result.IsHit && judgedObject.HitObject.Kiai)
                Ring.KiaiBeat();
        }
    }
}
