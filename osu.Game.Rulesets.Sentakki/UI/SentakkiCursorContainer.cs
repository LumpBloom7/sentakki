using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Sentakki.Skinning;
using osu.Game.Rulesets.Sentakki.Skinning.Default;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI;

public partial class SentakkiCursorContainer : GameplayCursorContainer
{
    protected override Drawable CreateCursor() => new SkinnableDrawable(new SentakkiSkinComponentLookup(SentakkiSkinComponents.Cursor), _ => new SentakkiCursor())
    {
        RelativeSizeAxes = Axes.None
    };
}
