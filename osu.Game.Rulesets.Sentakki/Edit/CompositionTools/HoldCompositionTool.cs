using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds;

namespace osu.Game.Rulesets.Sentakki.Edit.CompositionTools;

public class HoldCompositionTool : CompositionTool
{
    public HoldCompositionTool()
        : base("Hold")
    {
    }

    public override Drawable CreateIcon() => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Sliders);
    public override PlacementBlueprint CreatePlacementBlueprint() => new HoldPlacementBlueprint();
};
