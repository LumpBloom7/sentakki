using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds;

public partial class HoldSelectionBlueprint : SentakkiSelectionBlueprint<Hold, DrawableHold>
{
    private readonly HoldBody highlight;

    public override Quad SelectionQuad => highlight.ScreenSpaceDrawQuad;

    public HoldSelectionBlueprint(Hold item)
        : base(item)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        InternalChild = highlight = new HoldBody()
        {
            Colour = Color4.YellowGreen,
            Alpha = 0.5f,
        };
    }

    protected override void Update()
    {
        Rotation = HitObject.Lane.GetRotationForLane();
        highlight.Y = DrawableObject.NoteBody.Y;
        highlight.Scale = DrawableObject.NoteBody.Scale;
        highlight.Height = DrawableObject.NoteBody.Height;
    }
}
