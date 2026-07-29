using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy;

public class SentakkiLegacySkinTransformer(ISkin skin) : LegacySkinTransformer(skin)
{
    public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
    {
        switch (lookup)
        {
            case SkinComponentLookup<HitResult> resultComponent:
                var result = resultComponent.Component;

                if (Skin.GetAnimation($"sentakki/judgement-{result.GetDisplayNameForSentakkiResult()}", true, true) is null)
                    return null;

                return new LegacySentakkiJudgementPiece(result);

            case SentakkiSkinComponentLookup sentakkiComponent:

                switch (sentakkiComponent)
                {
                    default:
                        break;
                }

                break;
        }

        return base.GetDrawableComponent(lookup);
    }
}
