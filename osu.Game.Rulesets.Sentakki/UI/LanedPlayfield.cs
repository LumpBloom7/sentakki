using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.IO;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Rulesets.Sentakki.UI.Components.HitObjectLine;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class LanedPlayfield : Playfield
    {
        [Resolved]
        private DrawableSentakkiRuleset sentakkiRuleset { get; set; }

        public readonly List<Lane> Lanes = new List<Lane>();

        private readonly SortedDrawableProxyContainer slideBodyProxyContainer;
        private readonly SortedDrawableProxyContainer lanedNoteProxyContainer;

        public readonly LineRenderer HitObjectLineRenderer;

        [Cached]
        private readonly DrawablePool<SlideVisual.SlideChevron> chevronPool;

        private readonly DrawablePool<HitExplosion> explosionPool;

        private readonly Container<HitExplosion> explosionLayer;

        public readonly Container LanedHitObjectArea;

        public LanedPlayfield()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Origin = Anchor.Centre;

            for (int i = 0; i < 8; ++i)
            {
                var lane = new Lane
                {
                    Rotation = i.GetRotationForLane(),
                    LaneNumber = i,
                    OnLoaded = onHitObjectLoaded
                };
                Lanes.Add(lane);
                AddInternal(lane);
                AddNested(lane);
            }

            AddRangeInternal(new Drawable[]{
                explosionPool = new DrawablePool<HitExplosion>(8),
                chevronPool = new DrawablePool<SlideVisual.SlideChevron>(100),
                HitObjectLineRenderer = new LineRenderer(),
                explosionLayer = new Container<HitExplosion>() { RelativeSizeAxes = Axes.Both },
                slideBodyProxyContainer = new SortedDrawableProxyContainer(),
                LanedHitObjectArea = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = lanedNoteProxyContainer = new SortedDrawableProxyContainer(),
                }
            });

            NewResult += onNewResult;
        }

        public override void Add(HitObject h)
        {
            switch (h)
            {
                case SentakkiLanedHitObject laned:
                    HitObjectLineRenderer.AddHitObject(laned);
                    laned.LaneBindable.BindValueChanged(lane =>
                    {
                        if (lane.OldValue != lane.NewValue)
                            Lanes[lane.OldValue].Remove(h);
                        Lanes[lane.NewValue].Add(h);
                    }, true);
                    break;
            }
        }

        public override bool Remove(HitObject hitObject)
        {
            if (!(hitObject is SentakkiLanedHitObject lanedHitObject))
                return false;

            HitObjectLineRenderer.RemoveHitObject(lanedHitObject);
            return Lanes[lanedHitObject.Lane].Remove(lanedHitObject);
        }

        private void onHitObjectLoaded(Drawable hitObject)
        {
            switch (hitObject)
            {
                case DrawableSlideBody s:
                    slideBodyProxyContainer.Add(s.CreateProxy(), s);
                    break;
                case DrawableTap t:
                    lanedNoteProxyContainer.Add(t.TapVisual.CreateProxy(), t);
                    break;
                case DrawableHold h:
                    lanedNoteProxyContainer.Add(h.NoteBody.CreateProxy(), h);
                    break;
            }
        }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (!judgedObject.DisplayResult || !DisplayJudgements.Value || !(judgedObject is DrawableSentakkiLanedHitObject laned))
                return;

            if (!result.IsHit) return;

            if (judgedObject is DrawableSlideBody) return;

            TransmissionData.InfoType resultType = TransmissionData.InfoType.None;
            switch (result.Type)
            {
                case HitResult.Great:
                    resultType = TransmissionData.InfoType.HitPerfect;
                    break;
                case HitResult.Good:
                    resultType = TransmissionData.InfoType.HitGreat;
                    break;
                case HitResult.Meh:
                    resultType = TransmissionData.InfoType.HitGood;
                    break;
                case HitResult.Miss:
                    resultType = TransmissionData.InfoType.Miss;
                    break;
            }

            sentakkiRuleset.TryBroadcastGameplayEvent(new TransmissionData(resultType, laned.HitObject.Lane));

            var explosion = explosionPool.Get(e => e.Apply(laned.HitObject));
            explosionLayer.Add(explosion);
        }
    }
}
