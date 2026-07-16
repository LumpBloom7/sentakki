using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy;

public class SentakkiLegacySkinTransformer(ISkin skin) : LegacySkinTransformer(skin)
{
    public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
    {
        switch (lookup)
        {
            case GlobalSkinnableContainerLookup containerLookup:
                // Only handle per ruleset defaults here.
                if (containerLookup.Ruleset == null)
                    return base.GetDrawableComponent(lookup);

                // we don't have enough assets to display these components (this is especially the case on a "beatmap" skin).
                if (!IsProvidingLegacyResources)
                    return null;

                // Our own ruleset components default.
                switch (containerLookup.Lookup)
                {
                    case GlobalSkinnableContainers.MainHUDComponents:
                        return new DefaultSkinComponentsContainer(container =>
                        {
                            var keyCounter = container.OfType<LegacyKeyCounterDisplay>().FirstOrDefault();

                            if (keyCounter != null)
                            {
                                // set the anchor to top right so that it won't squash to the return button to the top
                                keyCounter.Anchor = Anchor.CentreRight;
                                keyCounter.Origin = Anchor.TopRight;
                                keyCounter.Position = new Vector2(0, -40) * 1.6f;
                            }

                            var combo = container.OfType<LegacyDefaultComboCounter>().FirstOrDefault();
                            var spectatorList = container.OfType<SpectatorList>().FirstOrDefault();
                            var leaderboard = container.OfType<DrawableGameplayLeaderboard>().FirstOrDefault();

                            Vector2 pos = new Vector2();

                            if (combo != null)
                            {
                                combo.Anchor = Anchor.BottomLeft;
                                combo.Origin = Anchor.BottomLeft;
                                combo.Scale = new Vector2(1.28f);

                                pos += new Vector2(10, -(combo.DrawHeight * 1.56f + 20) * combo.Scale.X);
                            }

                            if (spectatorList != null)
                            {
                                spectatorList.Anchor = Anchor.BottomLeft;
                                spectatorList.Origin = Anchor.BottomLeft;
                                spectatorList.Position = pos;

                                // maximum height of the spectator list is around ~172 units
                                pos += new Vector2(0, -185);
                            }

                            if (leaderboard != null)
                            {
                                leaderboard.Anchor = Anchor.BottomLeft;
                                leaderboard.Origin = Anchor.BottomLeft;
                                leaderboard.Position = pos;
                            }

                            foreach (var d in container.OfType<ISerialisableDrawable>())
                                d.UsesFixedAnchor = true;
                        })
                        {
                            Children =
                            [
                                    new LegacyDefaultComboCounter(),
                                    new LegacyKeyCounterDisplay(),
                                    new SpectatorList(),
                                    new DrawableGameplayLeaderboard(),
                            ]
                        };
                }
                return null;

            case SentakkiSkinComponentLookup sentakkiComponent:
                switch (sentakkiComponent.Component)
                {
                    case SentakkiSkinComponents.Tap:
                        if (GetTexture("sentakki-tap") == null)
                            return null;

                        return new LegacyTapPiece();
                }
                break;



        }
        return base.GetDrawableComponent(lookup);
    }
}
