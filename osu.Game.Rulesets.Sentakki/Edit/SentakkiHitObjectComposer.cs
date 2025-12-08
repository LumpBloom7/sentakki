using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Sentakki.Edit.CompositionTools;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class SentakkiHitObjectComposer : HitObjectComposer<SentakkiHitObject>
{
    public SentakkiHitObjectComposer(Ruleset ruleset)
        : base(ruleset)
    {
    }

    private DrawableRulesetDependencies dependencies = null!;

    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        => dependencies = new DrawableRulesetDependencies(Ruleset, base.CreateChildDependencies(parent));

    protected override ComposeBlueprintContainer CreateBlueprintContainer()
        => new SentakkiBlueprintContainer(this);

    protected override IEnumerable<Drawable> CreateTernaryButtons()
    {
        foreach (var ternaryButton in base.CreateTernaryButtons().Skip(1))
            yield return ternaryButton;

        var selectionHandler = (SentakkiSelectionHandler)BlueprintContainer.SelectionHandler;

        yield return new DrawableTernaryButton
        {
            Current = selectionHandler.BreakTernaryState,
            CreateIcon = () => new SpriteIcon { Icon = FontAwesome.Solid.WeightHanging },
            Description = "Break",
            TooltipText = "Increases the scoring weight of notes. Typically used to emphasize certain notes, or to increase punishment for inaccuracy."
        };

        yield return new DrawableTernaryButton
        {
            Current = selectionHandler.ExTernaryState,
            CreateIcon = () => new SpriteIcon { Icon = FontAwesome.Solid.Seedling },
            Description = "Ex",
            TooltipText = "Increases the judgement leniency of notes. Typically used to provide a safety net for players, allowing harder patterns to be introduced."
        };
    }

    protected override IReadOnlyList<CompositionTool> CompositionTools { get; } =
    [
        new TapCompositionTool(),
        new HoldCompositionTool(),
        new SlideCompositionTool(),
        new TouchCompositionTool(),
        new TouchHoldCompositionTool(),
    ];
}
