using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;

namespace osu.Game.Rulesets.Sentakki.Edit.CompositionTools;

public class SlideCompositionTool : CompositionTool
{
    public SlideCompositionTool()
        : base("Slide")
    {
    }

    public override Drawable CreateIcon() => new SpriteIcon { Icon = FontAwesome.Regular.Star };

    public override PlacementBlueprint CreatePlacementBlueprint() => new SlidePlacementBlueprint();
}
