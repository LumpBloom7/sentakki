using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
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
        public readonly List<Lane> Lanes = new List<Lane>();

        private readonly SortedDrawableProxyContainer slideBodyProxyContainer;
        private readonly SortedDrawableProxyContainer lanedNoteProxyContainer;

        private readonly LineRenderer hitObjectLineRenderer;

        [Cached]
        private readonly DrawablePool<SlideVisual.SlideChevron> chevronPool;

        private DrawablePool<HitExplosion> explosionPool;

        private readonly Container<HitExplosion> explosionLayer;

        public LanedPlayfield()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AddInternal(hitObjectLineRenderer = new LineRenderer());
            AddInternal(explosionLayer = new Container<HitExplosion>() { RelativeSizeAxes = Axes.Both });



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

            AddInternal(slideBodyProxyContainer = new SortedDrawableProxyContainer());
            AddInternal(lanedNoteProxyContainer = new SortedDrawableProxyContainer());
            AddInternal(chevronPool = new DrawablePool<SlideVisual.SlideChevron>(100));

            NewResult += onNewResult;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(explosionPool = new DrawablePool<HitExplosion>(8));
        }

        public override void Add(HitObject h)
        {
            switch (h)
            {
                case SentakkiLanedHitObject laned:
                    hitObjectLineRenderer.AddHitObject(laned);
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
            hitObjectLineRenderer.RemoveHitObject((SentakkiLanedHitObject)hitObject);
            return Lanes[(hitObject as SentakkiLanedHitObject).Lane].Remove(hitObject);
        }

        private void onHitObjectLoaded(Drawable hitObject)
        {
            switch (hitObject)
            {
                case DrawableSlide s:
                    foreach (var x in s.SlideBodies)
                        slideBodyProxyContainer.Add(x.CreateProxy(), s);
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

            var explosion = explosionPool.Get(e => e.Apply(laned.HitObject));
            explosionLayer.Add(explosion);
        }
    }
}
