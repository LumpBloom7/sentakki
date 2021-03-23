using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class LanedPlayfield : Playfield
    {
        public readonly List<Lane> Lanes = new List<Lane>();

        private readonly SortedDrawableProxyContainer slideBodyProxyContainer;
        private readonly SortedDrawableProxyContainer lanedNoteProxyContainer;

        [Cached]
        private readonly DrawablePool<SlideVisual.SlideChevron> chevronPool;

        public LanedPlayfield()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

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
        }

        public override void Add(HitObject h)
        {
            switch (h)
            {
                case SentakkiLanedHitObject laned:
                    laned.LaneBindable.BindValueChanged(lane =>
                    {
                        if (lane.OldValue != lane.NewValue)
                            Lanes[lane.OldValue].Remove(h);
                        Lanes[lane.NewValue].Add(h);
                    }, true);
                    break;
            }
        }

        public override bool Remove(HitObject hitObject) => Lanes[(hitObject as SentakkiLanedHitObject).Lane].Remove(hitObject);

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
    }
}
