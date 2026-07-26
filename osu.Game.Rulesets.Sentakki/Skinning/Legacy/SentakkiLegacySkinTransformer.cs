using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Skinning.Legacy.TouchHold;
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

                    case SentakkiSkinComponents.Touch:
                        if (GetTexture("sentakki/touch") is null)
                            return null;

                        return new LegacyTouchBody();

                    case SentakkiSkinComponents.TouchHold:
                        if (GetTexture("sentakki/touchhold") is null)
                            return null;

                        return new LegacyTouchHoldBody();

                    case SentakkiSkinComponents.SlideStar:
                        if (GetTexture("sentakki/star") is null)
                            return null;

                        return new LegacyStarPiece();

                    case SentakkiSkinComponents.SlideChevron:
                        if (GetTexture("sentakki/slide-chevron") is null)
                            return null;

                        return new LegacySlideChevron();

                    case SentakkiSkinComponents.SlideFanChevron0:
                    case SentakkiSkinComponents.SlideFanChevron1:
                    case SentakkiSkinComponents.SlideFanChevron2:
                    case SentakkiSkinComponents.SlideFanChevron3:
                    case SentakkiSkinComponents.SlideFanChevron4:
                    case SentakkiSkinComponents.SlideFanChevron5:
                    case SentakkiSkinComponents.SlideFanChevron6:
                    case SentakkiSkinComponents.SlideFanChevron7:
                    case SentakkiSkinComponents.SlideFanChevron8:
                    case SentakkiSkinComponents.SlideFanChevron9:
                    case SentakkiSkinComponents.SlideFanChevron10:
                        int index = (int)sentakkiComponent.Component - (int)SentakkiSkinComponents.SlideFanChevron0;

                        if (GetTexture($"sentakki/slide-fan-chevron-{index}") is null)
                            return null;

                        return new LegacySlideFanChevron(index);
                }
                break;
        }

        return base.GetDrawableComponent(lookup);
    }
}
