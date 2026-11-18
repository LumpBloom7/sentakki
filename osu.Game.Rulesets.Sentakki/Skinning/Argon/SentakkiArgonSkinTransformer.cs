using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Skinning.Argon.HUD;
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

            case GlobalSkinnableContainerLookup containerLookup:
                // Only handle per ruleset defaults here.
                if (containerLookup.Ruleset == null)
                    break;

                // Our own ruleset components default.
                switch (containerLookup.Lookup)
                {
                    case GlobalSkinnableContainers.Playfield:
                        return new DefaultSkinComponentsContainer(c =>
                        {
                            var contextualCounter = c.OfType<ArgonContextualCounter>().FirstOrDefault();

                            contextualCounter?.Anchor = Anchor.Centre;
                            contextualCounter?.Origin = Anchor.Centre;
                        })
                        {
                            Children =
                            [
                                new ArgonContextualCounter()
                            ]
                        };
                }

                break;
        }

        return base.GetDrawableComponent(lookup);
    }
}
