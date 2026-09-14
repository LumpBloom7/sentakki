using osu.Framework.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy;

public class SentakkiLegacySkinTransformer(ISkin skin) : LegacySkinTransformer(skin)
{
    public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
    {
        switch (lookup)
        {
            case SentakkiSkinComponentLookup sentakkiComponent:
                switch (sentakkiComponent.Component)
                {
                    case SentakkiSkinComponents.Tap:
                        if (Skin.GetTexture("sentakki/hitobjects/tap/base") is null)
                            return null;

                        return new LegacyTapRing();

                    default:
                        break;
                }

                break;
        }

        return base.GetDrawableComponent(lookup);
    }
}
