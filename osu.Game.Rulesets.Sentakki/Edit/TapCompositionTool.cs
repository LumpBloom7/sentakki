using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class TapCompositionTool : HitObjectCompositionTool
    {
        public TapCompositionTool()
            : base(nameof(Tap))
        {
        }

        public override PlacementBlueprint CreatePlacementBlueprint() => new TapPlacementBlueprint();
    }
}