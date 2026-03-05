using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Touches;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touches;

public partial class TouchSelectionBlueprint : SentakkiSelectionBlueprint<Touch, DrawableTouch>
{
    private readonly TouchBody highlight;

    public override Quad SelectionQuad => highlight.ScreenSpaceDrawQuad;

    public TouchSelectionBlueprint(Touch item)
        : base(item)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        InternalChild = highlight = new TouchBody()
        {
            Alpha = 0.5f,
            Colour = Color4.YellowGreen,
        };
    }

    protected override void Update()
    {
        base.Update();

        highlight.Position = DrawableObject.Position;

        highlight.Size = DrawableObject.TouchBody.Size;
        highlight.BorderContainer.Alpha = DrawableObject.TouchBody.BorderContainer.Alpha;
    }
}
