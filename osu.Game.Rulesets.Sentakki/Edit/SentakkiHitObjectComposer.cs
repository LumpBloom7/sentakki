using System.Collections.Generic;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class SentakkiHitObjectComposer : HitObjectComposer<SentakkiHitObject>
    {
        public SentakkiHitObjectComposer(SentakkiRuleset ruleset)
            : base(ruleset)
        {
        }

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new HitObjectCompositionTool[]
        {
            new TapCompositionTool(),
            new HoldCompositionTool(),
        };

        protected override ComposeBlueprintContainer CreateBlueprintContainer() => new SentakkiBlueprintContainer(this);
    }
}
