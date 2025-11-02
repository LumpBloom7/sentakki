using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touches;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class SentakkiBlueprintContainer : ComposeBlueprintContainer
{
    public SentakkiBlueprintContainer(HitObjectComposer composer)
        : base(composer)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
    }

    public override HitObjectSelectionBlueprint? CreateHitObjectBlueprintFor(HitObject hitObject)
    {
        switch (hitObject)
        {
            case Tap tap:
                return new TapSelectionBlueprint(tap);

            case Hold hold:
                return new HoldSelectionBlueprint(hold);

            case Touch touch:
                return new TouchSelectionBlueprint(touch);

            case TouchHold touchHold:
                return new TouchHoldSelectionBlueprint(touchHold);

            default:
                return base.CreateHitObjectBlueprintFor(hitObject);
        }
    }

    protected override bool TryMoveBlueprints(DragEvent e, IList<(SelectionBlueprint<HitObject> blueprint, Vector2[] originalSnapPositions)> blueprints)
    {
        return false;
    }
}
