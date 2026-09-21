using System;
using osu.Framework.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.Skinning;

public partial class ProxyableSkinnableDrawable(ISkinComponentLookup lookup, Func<ISkinComponentLookup, Drawable>? defaultImplementation = null, ConfineMode confineMode = ConfineMode.NoScaling)
    : SkinnableDrawable(lookup, defaultImplementation, confineMode)
{
    public override bool RemoveWhenNotAlive => false;
}
