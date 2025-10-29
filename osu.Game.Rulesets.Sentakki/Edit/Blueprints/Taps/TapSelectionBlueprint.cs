using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps;

public partial class TapSelectionBlueprint : SentakkiSelectionBlueprint<Tap>
{
    public new DrawableTap DrawableObject => (DrawableTap)base.DrawableObject;

    private readonly TapHighlight highlight;

    public TapSelectionBlueprint(Tap hitObject)
        : base(hitObject)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        InternalChild = highlight = new TapHighlight()
        {
            Colour = Color4.YellowGreen,
            Alpha = 0.5f,
        };
    }

    [Resolved]
    private SentakkiSnapProvider snapProvider { get; set; } = null!;

    protected override void Update()
    {
        base.Update();

        Rotation = DrawableObject.HitObject.Lane.GetRotationForLane();
        highlight.Y = -snapProvider.GetDistanceRelativeToCurrentTime(DrawableObject.HitObject.StartTime, SentakkiPlayfield.NOTESTARTDISTANCE);
        highlight.Scale = DrawableObject.TapVisual.Scale;
    }

    public override Vector2 ScreenSpaceSelectionPoint => highlight.ScreenSpaceDrawQuad.Centre;

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => highlight.ReceivePositionalInputAt(screenSpacePos);

    public override Quad SelectionQuad => highlight.ScreenSpaceDrawQuad;
}
