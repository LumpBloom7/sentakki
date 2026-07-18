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
                        if (GetTexture("sentakki/tap") == null)
                            return null;

                        return new LegacyTapPiece();

                    case SentakkiSkinComponents.Hold:
                        if (GetTexture("sentakki/hold") is null)
                            return null;

                        return new LegacyHoldBody();
                }
                break;


        }

        return base.GetDrawableComponent(lookup);
    }
}
