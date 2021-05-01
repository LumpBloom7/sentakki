using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touches;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class TouchCompositionTool : HitObjectCompositionTool
    {
        public TouchCompositionTool()
            : base(nameof(Touch))
        {
        }

        public override Drawable CreateIcon() => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Circles);

        public override PlacementBlueprint CreatePlacementBlueprint() => new TouchPlacementBlueprint();
    }
}
