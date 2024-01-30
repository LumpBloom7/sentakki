using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.UI.Components.HitObjectLine;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public partial class LanedPlayfield : Playfield
    {
        public readonly List<Lane> Lanes = new List<Lane>();

        private readonly SortedDrawableProxyContainer slideBodyProxyContainer;
        private readonly SortedDrawableProxyContainer slideStarProxyContainer;
        private readonly SortedDrawableProxyContainer lanedNoteProxyContainer;

        public readonly LineRenderer HitObjectLineRenderer;

        [Cached]
        private readonly DrawablePool<SlideChevron> chevronPool;

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

            AddRangeInternal(new Drawable[]
            {
                chevronPool = new DrawablePool<SlideChevron>(100),
                HitObjectLineRenderer = new LineRenderer(),
                slideBodyProxyContainer = new SortedDrawableProxyContainer(),
                slideStarProxyContainer = new SortedDrawableProxyContainer(),
                LanedHitObjectArea = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = lanedNoteProxyContainer = new SortedDrawableProxyContainer(),
                }
            });
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
            if (hitObject is not SentakkiLanedHitObject lanedHitObject)
                return false;

            HitObjectLineRenderer.RemoveHitObject(lanedHitObject);
            return Lanes[lanedHitObject.Lane].Remove(lanedHitObject);
        }

        private void onHitObjectLoaded(Drawable hitObject)
        {
            switch (hitObject)
            {
                case DrawableSlideBody s:
                    slideBodyProxyContainer.Add(s.Slidepath.CreateProxy(), s);
                    slideStarProxyContainer.Add(s.SlideStars.CreateProxy(), s);
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
