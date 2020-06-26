using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class TouchHoldCompositionTool : HitObjectCompositionTool
    {
        public TouchHoldCompositionTool()
            : base(nameof(TouchHold))
        {
        }

        public override PlacementBlueprint CreatePlacementBlueprint() => new TouchHoldPlacementBlueprint();
    }
}
