using System.Collections.Generic;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using System;
using osu.Framework.Graphics;

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
            new TouchHoldCompositionTool(),
            new TouchCompositionTool(),
            new SlideCompositionTool(),
        };

        protected override ComposeBlueprintContainer CreateBlueprintContainer()
            => new SentakkiBlueprintContainer(this);
    }
}
