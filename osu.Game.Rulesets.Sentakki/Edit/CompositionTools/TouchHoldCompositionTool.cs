using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds;
using osu.Game.Rulesets.Sentakki.Localisation;

namespace osu.Game.Rulesets.Sentakki.Edit.CompositionTools;

public class TouchHoldCompositionTool : CompositionTool<SentakkiAction>
{
    public TouchHoldCompositionTool()
        : base(SentakkiEditorStrings.TouchHoldTool)
    {
        Action = SentakkiAction.EditorTouchHoldTool;
    }

    public override Drawable CreateIcon() => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Spinners);
    public override PlacementBlueprint CreatePlacementBlueprint() => new TouchHoldPlacementBlueprint();
}
