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

                Drawable? drawable = Skin.GetAnimation($"sentakki/judgements/{result.GetDisplayNameForSentakkiResult().ToLowerInvariant()}", true, false, animationSeparator: "/");

                if (drawable is null)
                    return null;

                return new LegacySentakkiJudgementPiece(result, drawable);

            case SentakkiSkinComponentLookup sentakkiComponent:
                switch (sentakkiComponent.Component)
                {
                    default:
                        break;
                }

                break;
        }

        return base.GetDrawableComponent(lookup);
    }
}
