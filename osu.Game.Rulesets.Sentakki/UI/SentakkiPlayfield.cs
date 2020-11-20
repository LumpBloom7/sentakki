using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Input;
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
    public class SentakkiPlayfield : Playfield, IRequireHighFrequencyMousePosition
    {
        private readonly JudgementContainer<DrawableSentakkiJudgement> judgementLayer;

        private readonly SentakkiRing ring;
        public BindableNumber<int> RevolutionDuration = new BindableNumber<int>(0);

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public static readonly float RINGSIZE = 600;
        public static readonly float DOTSIZE = 20f;
        public static readonly float INTERSECTDISTANCE = 296.5f;
        public static readonly float NOTESTARTDISTANCE = 66f;

        private readonly LanedPlayfield lanedPlayfield;

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
                new PlayfieldVisualisation(),
                ring = new SentakkiRing(),
                lanedPlayfield = new LanedPlayfield(),
                HitObjectContainer, // This only contains Touch and TouchHolds, which should appear above others note types. Might consider separating to another playfield.
                judgementLayer = new JudgementContainer<DrawableSentakkiJudgement>
                {
                    RelativeSizeAxes = Axes.Both,
                }
            });
            AddNested(lanedPlayfield);
        }

        private DrawableSentakkiRuleset drawableSentakkiRuleset;
        private SentakkiRulesetConfigManager sentakkiRulesetConfig;

        [BackgroundDependencyLoader(true)]
        private void load(DrawableSentakkiRuleset drawableRuleset, SentakkiRulesetConfigManager sentakkiRulesetConfigManager)
        {
            drawableSentakkiRuleset = drawableRuleset;
            sentakkiRulesetConfig = sentakkiRulesetConfigManager;


            registerPool<TouchHold, DrawableTouchHold>(1, 1);

            registerPool<Objects.Touch, DrawableTouch>(8);
        }

        private void registerPool<TObject, TDrawable>(int initialSize, int? maximumSize = null)
            where TObject : HitObject
            where TDrawable : DrawableHitObject, new()
            => RegisterPool<TObject, TDrawable>(CreatePool<TDrawable>(initialSize, maximumSize));

        protected virtual DrawablePool<TDrawable> CreatePool<TDrawable>(int initialSize, int? maximumSize = null)
            where TDrawable : DrawableHitObject, new()
            => new DrawableSentakkiPool<TDrawable>(null, initialSize, maximumSize);

        protected override HitObjectLifetimeEntry CreateLifetimeEntry(HitObject hitObject) => new SentakkiHitObjectLifetimeEntry(hitObject, sentakkiRulesetConfig, drawableSentakkiRuleset);

        protected override void Update()
        {
            // Using deltaTime instead of what I did with the hitObjects to avoid noticible jitter during rate changed.
            if (RevolutionDuration.Value > 0)
            {
                double rotationAmount = Clock.ElapsedFrameTime / (RevolutionDuration.Value * 1000 * (drawableSentakkiRuleset?.GameplaySpeed ?? 1)) * 360;
                Rotation += (float)rotationAmount;
            }
            base.Update();
        }

        protected override GameplayCursorContainer CreateCursor() => new SentakkiCursorContainer();

        public override void Add(HitObject h)
        {
            switch (h)
            {
                case SentakkiLanedHitObject _:
                    lanedPlayfield.Add(h);
                    break;

                default:
                    base.Add(h);
                    break;
            }
        }

        public override void Add(DrawableHitObject h)
        {
            h.OnNewResult += onNewResult;
            switch (h)
            {
                case DrawableTap _:
                case DrawableHold _:
                case DrawableSlide _:
                    lanedPlayfield.Add(h);
                    break;

                default:
                    base.Add(h);
                    break;
            }
        }

        public override bool Remove(DrawableHitObject h)
        {
            switch (h)
            {
                case DrawableTap _:
                case DrawableHold _:
                case DrawableSlide _:
                    return lanedPlayfield.Remove(h);

                default:
                    return base.Remove(h);
            }
        }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (!judgedObject.DisplayResult || !DisplayJudgements.Value)
                return;

            if (!(judgedObject is DrawableSentakkiHitObject)) return;

            var sentakkiObj = judgedObject as DrawableSentakkiHitObject;

            DrawableSentakkiJudgement explosion;
            switch (judgedObject.HitObject)
            {
                case SentakkiLanedHitObject laned:
                    explosion = new DrawableSentakkiJudgement(result, sentakkiObj)
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Position = SentakkiExtensions.GetPositionAlongLane(240, laned.Lane),
                        Rotation = laned.Lane.GetRotationForLane(),
                    };
                    break;

                default:
                    explosion = new DrawableSentakkiJudgement(result, sentakkiObj)
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Position = sentakkiObj.Position
                    };
                    break;
            }

            judgementLayer.Add(explosion);

            if (result.IsHit && judgedObject.HitObject.Kiai)
                ring.KiaiBeat();
        }
    }
}
