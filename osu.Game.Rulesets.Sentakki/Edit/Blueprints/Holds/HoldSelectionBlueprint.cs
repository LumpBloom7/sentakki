using osu.Framework.Allocation;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds;

public partial class HoldSelectionBlueprint : SentakkiSelectionBlueprint<Hold>
{
    public new DrawableHold DrawableObject => (DrawableHold)base.DrawableObject;

    private readonly HoldHighlight highlight;

    public HoldSelectionBlueprint(Hold hitObject)
        : base(hitObject)
    {
        InternalChild = highlight = new HoldHighlight();
    }

    [Resolved]
    private SentakkiSnapProvider snapProvider { get; set; } = null!;

    protected override void Update()
    {
        base.Update();

        highlight.Rotation = DrawableObject.HitObject.Lane.GetRotationForLane();
        highlight.Note.Y = -snapProvider.GetDistanceRelativeToCurrentTime(HitObject.StartTime, SentakkiPlayfield.NOTESTARTDISTANCE);
        highlight.Note.Height = -snapProvider.GetDistanceRelativeToCurrentTime(HitObject.EndTime, SentakkiPlayfield.NOTESTARTDISTANCE) - highlight.Note.Y;
        highlight.Note.Scale = DrawableObject.NoteBody.Scale;
    }

    public override Vector2 ScreenSpaceSelectionPoint => highlight.ScreenSpaceDrawQuad.Centre;

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => highlight.ReceivePositionalInputAt(screenSpacePos);

    public override Quad SelectionQuad => highlight.ScreenSpaceDrawQuad;
}
