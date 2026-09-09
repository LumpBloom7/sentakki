using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.Skinning.Argon;

public partial class SentakkiArgonSkinTransformer(ISkin skin) : SkinTransformer(skin)
{
    public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
    {
        bool isPro = Skin is ArgonProSkin;

        switch (lookup)
        {
            case SkinComponentLookup<HitResult> resultComponent:
                switch (resultComponent.Component)
                {
                    case HitResult.Perfect or HitResult.Great when isPro:
                        return Drawable.Empty();

                    default:
                        return new ArgonSentakkiJudgementPiece(resultComponent.Component);
                }
        }

        return base.GetDrawableComponent(lookup);
    }
}
