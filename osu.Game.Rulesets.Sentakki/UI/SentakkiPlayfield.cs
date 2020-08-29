using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osuTK;
using System;

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

        // Touch notes always appear above other notes, regardless of start time
        private readonly TouchNoteProxyContainer touchNoteContainer;

        // Slide body always appear under other notes, regardless of start time
        private readonly SlideBodyProxyContainer slidebodyContainer;

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
                slidebodyContainer = new  SlideBodyProxyContainer(),
                HitObjectContainer,
                touchNoteContainer = new TouchNoteProxyContainer(),
                judgementLayer = new JudgementContainer<DrawableSentakkiJudgement>
                {
                    RelativeSizeAxes = Axes.Both,
                }
            });
        }

        private DrawableSentakkiRuleset drawableSentakkiRuleset;

        [BackgroundDependencyLoader(true)]
        private void load(DrawableSentakkiRuleset drawableRuleset)
        {
            drawableSentakkiRuleset = drawableRuleset;
        }

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

        public override void Add(DrawableHitObject h)
        {
            h.OnNewResult += onNewResult;
            h.OnLoadComplete += d =>
            {
                if (d is IDrawableHitObjectWithProxiedApproach c)
                    switch (d)
                    {
                        case DrawableSlide _:
                            slidebodyContainer.Add(c.ProxiedLayer.CreateProxy());
                            break;
                        case DrawableTouch _:
                            touchNoteContainer.Add(c.ProxiedLayer.CreateProxy());
                            break;
                    }
            };
            base.Add(h);
        }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (!judgedObject.DisplayResult || !DisplayJudgements.Value)
                return;

            var sentakkiObj = (DrawableSentakkiHitObject)judgedObject;

            DrawableSentakkiJudgement explosion;
            switch (judgedObject)
            {
                case DrawableTouch t:
                    explosion = new DrawableSentakkiJudgement(result, sentakkiObj)
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Position = t.Position
                    };
                    break;

                case DrawableTouchHold _:
                    explosion = new DrawableSentakkiJudgement(result, sentakkiObj)
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                    };
                    break;
                default:
                    explosion = new DrawableSentakkiJudgement(result, sentakkiObj)
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Position = SentakkiExtensions.GetPositionAlongLane(240, sentakkiObj.HitObject.Lane),
                        Rotation = sentakkiObj.HitObject.Lane.GetRotationForLane(),
                    };
                    break;
            }

            judgementLayer.Add(explosion);

            if (result.IsHit && judgedObject.HitObject.Kiai)
                ring.KiaiBeat();
        }

        private class TouchNoteProxyContainer : LifetimeManagementContainer
        {
            public TouchNoteProxyContainer()
            {
                RelativeSizeAxes = Axes.Both;
            }
            public void Add(Drawable touchNoteProxy) => AddInternal(touchNoteProxy);
        }

        private class SlideBodyProxyContainer : LifetimeManagementContainer
        {
            public SlideBodyProxyContainer()
            {
                RelativeSizeAxes = Axes.Both;
            }
            public void Add(Drawable slideBodyProxy) => AddInternal(slideBodyProxy);
        }
    }
}
