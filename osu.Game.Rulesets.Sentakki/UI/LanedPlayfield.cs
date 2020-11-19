using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class LanedPlayfield : Playfield
    {
        public readonly List<Lane> Lanes = new List<Lane>();

        private readonly SortedDrawableProxyContainer slideBodyProxyContainer;
        private readonly SortedDrawableProxyContainer lanedNoteProxyContainer;

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
                    OnLoaded = OnHitObjectLoaded
                };
                Lanes.Add(lane);
                AddInternal(lane);
                AddNested(lane);
            }

            AddInternal(slideBodyProxyContainer = new SortedDrawableProxyContainer());
            AddInternal(lanedNoteProxyContainer = new SortedDrawableProxyContainer());
        }

        public void OnHitObjectLoaded(Drawable hitObject)
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

        public override void Add(HitObject h)
        {
            switch (h)
            {
                case SentakkiLanedHitObject laned:
                    Lanes[laned.Lane].Add(h);
                    break;

                default:
                    base.Add(h);
                    break;
            }
        }

        public override void Add(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableSlide s:
                    foreach (var x in s.SlideBodies)
                        slideBodyProxyContainer.Add(x.CreateProxy(), hitObject);
                    break;
                case DrawableTap t:
                    lanedNoteProxyContainer.Add(t.TapVisual.CreateProxy(), hitObject);
                    break;
                case DrawableHold h:
                    lanedNoteProxyContainer.Add(h.NoteBody.CreateProxy(), hitObject);
                    break;
            }

            ((SentakkiLanedHitObject)hitObject.HitObject).LaneBindable.BindValueChanged(lane =>
            {
                if (lane.OldValue != lane.NewValue)
                    Lanes[lane.OldValue].Remove(hitObject);
                Lanes[lane.NewValue].Add(hitObject);
            }, true);
        }

        public override bool Remove(DrawableHitObject hitObject) => Lanes[(hitObject.HitObject as SentakkiLanedHitObject).Lane].Remove(hitObject);
    }
}
