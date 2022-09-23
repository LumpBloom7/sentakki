using System;
using osu.Framework.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.Skinning
{
    public class ProxyableSkinnableDrawable : SkinnableDrawable
    {
        public override bool RemoveWhenNotAlive => false;

        public ProxyableSkinnableDrawable(ISkinComponent component, Func<ISkinComponent, Drawable> defaultImplementation = null, ConfineMode confineMode = ConfineMode.NoScaling)
            : base(component, defaultImplementation, confineMode)
        {
        }
    }
}
