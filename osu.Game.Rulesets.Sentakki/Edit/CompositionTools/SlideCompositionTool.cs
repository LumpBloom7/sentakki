using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit.CompositionTools;

public class SlideCompositionTool : CompositionTool
{
    public SlideCompositionTool()
        : base(nameof(Slide))
    {
        TooltipText =
            """
            Left click to place segment.
            Tab, Shift-Tab, or Alt-1~7 to change current segment type.
            Ctrl-Tab, or repeat Alt-1~7 to mirror segment (if applicable).
            A/S/D to decrease/reset/increase shoot offset.
            Right click to finish.
            """;
    }

    public override Drawable CreateIcon() => new SpriteIcon
    {
        Icon = FontAwesome.Regular.Star,
    };

    public override PlacementBlueprint CreatePlacementBlueprint() => new SlidePlacementBlueprint();
}
