using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touches;

namespace osu.Game.Rulesets.Sentakki.Edit.CompositionTools;

public class TouchCompositionTool : CompositionTool
{
    public TouchCompositionTool()
        : base("Touch")
    {
    }

    public override Drawable CreateIcon() => new SpriteIcon { Icon = FontAwesome.Regular.HandPointRight };

    public override PlacementBlueprint CreatePlacementBlueprint() => new TouchPlacementBlueprint();
}
