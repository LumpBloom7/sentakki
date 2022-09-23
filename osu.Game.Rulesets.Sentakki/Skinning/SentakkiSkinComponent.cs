using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.Skinning
{
    public class SentakkiSkinComponent : GameplaySkinComponent<SentakkiSkinComponents>
    {
        public SentakkiSkinComponent(SentakkiSkinComponents component)
            : base(component)
        {
        }

        protected override string RulesetPrefix => SentakkiRuleset.SHORT_NAME;

        protected override string ComponentName => Component.ToString().ToLowerInvariant();
    }
}
