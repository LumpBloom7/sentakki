using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touchs;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class TouchCompositionTool : HitObjectCompositionTool
    {
        public TouchCompositionTool()
            : base(nameof(Touch))
        {
        }

        public override PlacementBlueprint CreatePlacementBlueprint() => new TouchPlacementBlueprint();
    }
}
