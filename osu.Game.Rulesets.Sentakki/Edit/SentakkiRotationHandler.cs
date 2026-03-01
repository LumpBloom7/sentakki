using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class SentakkiRotationHandler : SelectionRotationHandler
{
    [Resolved]
    private EditorBeatmap editorBeatmap { get; set; } = null!;

    [Resolved]
    private SentakkiHitObjectComposer composer { get; set; } = null!;

    public SentakkiRotationHandler()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        DefaultOrigin = Vector2.Zero;

        CanRotateAroundPlayfieldOrigin.Value = true;

        // We really only care about rotating around playfield origin
        // But we still want to show the convenient button and allow the use of keybinds.
        CanRotateAroundSelectionOrigin.Value = true;

        OperationInProgress.Value = true;
    }


    public override void Update(float rotation, Vector2? origin = null)
    {
        base.Update(rotation, origin);

        int laneShift = (int)Math.Round(rotation / 45);

        float theta = laneShift * 45f / 180 * MathF.PI;
        (float sin, float cos) = MathF.SinCos(theta);

        editorBeatmap.BeginChange();

        foreach (var item in editorBeatmap.SelectedHitObjects)
        {
            switch (item)
            {
                case SentakkiLanedHitObject laned:
                    composer.Playfield.Remove(laned);

                    laned.Lane = (laned.Lane + laneShift).NormalizeLane();

                    editorBeatmap.Update(laned);
                    composer.Playfield.Add(laned);

                    break;

                case IHasPosition touch:
                    var pos = touch.Position;

                    touch.Position = new Vector2
                    {
                        X = cos * pos.X - sin * pos.Y,
                        Y = sin * pos.X + cos * pos.Y
                    };

                    editorBeatmap.Update(item);
                    break;
            }
        }

        editorBeatmap.EndChange();
    }

    // HACK: For some reason, rotation handles do not explicitly specify that they want to rotate around the selection origin
    // Yet they always come free with the rotation buttons
    // Let's fool the handlers into thinking a rotation is always happening, so they don't even attempt to do continuous rotation
    public override void Begin()
    {
    }

    public override void Commit()
    {
    }
}
