using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Skinning.Default.HUD;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default;

public partial class SentakkiDefaultSkinTransformer(ISkin skin) : SkinTransformer(skin)
{
    public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
    {
        switch (lookup)
        {
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
                            var contextualCounter = c.OfType<DefaultContextualCounter>().FirstOrDefault();

                            contextualCounter?.Anchor = Anchor.Centre;
                            contextualCounter?.Origin = Anchor.Centre;
                        })
                        {
                            Children =
                            [
                                new DefaultContextualCounter()
                            ]
                        };
                }

                break;
        }

        return base.GetDrawableComponent(lookup);
    }
}
