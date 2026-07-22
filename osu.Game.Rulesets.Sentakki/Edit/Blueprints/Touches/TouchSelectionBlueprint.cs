using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Skinning;
using osu.Game.Rulesets.Sentakki.Skinning.Common;
using osu.Game.Rulesets.Sentakki.Skinning.Default.Touch;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touches;

public partial class TouchSelectionBlueprint : SentakkiSelectionBlueprint<Touch, DrawableTouch>
{
    private readonly SkinnableDrawable highlight;

    public TouchSelectionBlueprint(Touch item)
        : base(item)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.None;
        Size = new Vector2(130);

        InternalChild = highlight = new SkinnableDrawable(new SentakkiSkinComponentLookup(SentakkiSkinComponents.Touch), _ => new TouchBody(), ConfineMode.ScaleToFit)
        {
            Alpha = 0.5f,
            Colour = Color4.YellowGreen,
        };
    }

    protected override void Update()
    {
        base.Update();

        Position = DrawableObject.Position;
        Size = DrawableObject.TouchBody.Size;

        ((ITouchBody)highlight.Drawable).Border.Alpha = ((ITouchBody)DrawableObject.TouchBody.Drawable).Border.Alpha;
    }
}
