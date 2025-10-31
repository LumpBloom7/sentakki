using System.Collections.Generic;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Sentakki.Edit.CompositionTools;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class SentakkiHitObjectComposer : HitObjectComposer<SentakkiHitObject>
{
    public SentakkiHitObjectComposer(Ruleset ruleset)
        : base(ruleset)
    {
    }

    protected override ComposeBlueprintContainer CreateBlueprintContainer()
        => new SentakkiBlueprintContainer(this);

    protected override IReadOnlyList<CompositionTool> CompositionTools { get; } = [new TapCompositionTool()];
}
