using osu.Framework.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy
{
    public class SentakkiLegacySkinTransformer : LegacySkinTransformer
    {
        public SentakkiLegacySkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override Drawable? GetDrawableComponent(ISkinComponent component) => null;
    }
}
