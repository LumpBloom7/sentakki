using System.Collections.Generic;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class SentakkiBlueprintContainer : ComposeBlueprintContainer
{
    public SentakkiBlueprintContainer(HitObjectComposer composer)
        : base(composer)
    {
    }

    public override HitObjectSelectionBlueprint CreateHitObjectBlueprintFor(HitObject hitObject)
    {
        switch (hitObject)
        {
            case Tap t:
                return new TapSelectionBlueprint(t);

            default:
                return base.CreateHitObjectBlueprintFor(hitObject);
        }
    }

    protected override bool TryMoveBlueprints(DragEvent e, IList<(SelectionBlueprint<HitObject> blueprint, Vector2[] originalSnapPositions)> blueprints)
    {
        return false;
    }
}
