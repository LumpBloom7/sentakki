using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.UI;
using System;
using System.Linq;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class LanedPlayfield : Playfield
    {
        public readonly List<Lane> Lanes = new List<Lane>();

        private readonly SlideBodyProxyContainer slideBodyProxyContainer;

        public LanedPlayfield()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            AddInternal(slideBodyProxyContainer = new SlideBodyProxyContainer());

            foreach (var angle in SentakkiPlayfield.LANEANGLES)
            {
                var lane = new Lane { Rotation = angle, };
                Lanes.Add(lane);
                AddInternal(lane);
                AddNested(lane);
            }
        }

        public override void Add(DrawableHitObject hitObject)
        {
            if (hitObject is DrawableSlide s)
                slideBodyProxyContainer.Add(s.ProxiedLayer.CreateProxy());

            Lanes[(hitObject.HitObject as SentakkiLanedHitObject).Lane].Add(hitObject);
        }

        public override bool Remove(DrawableHitObject hitObject) => Lanes[(hitObject.HitObject as SentakkiLanedHitObject).Lane].Remove(hitObject);

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