using System;
using System.Collections.Generic;
using System.Linq;
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
using osu.Game.Utils;
using osuTK;
namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class SentakkiRotationHandler : SelectionRotationHandler
{
    [Resolved]
    private EditorBeatmap editorBeatmap { get; set; } = null!;

    [Resolved]
    private SentakkiHitObjectComposer composer { get; set; } = null!;

    private BindableList<HitObject> selectedHitObjects = null!;

    public SentakkiRotationHandler()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        CanRotateAroundPlayfieldOrigin.Value = true;

        // We really only care about rotating around playfield origin
        // But we still want to show the convenient button and allow the use of keybinds.
        CanRotateAroundSelectionOrigin.Value = true;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        selectedHitObjects = editorBeatmap.SelectedHitObjects.GetBoundCopy();
        selectedHitObjects.BindCollectionChanged((_, _) => updateState());
    }

    private void updateState()
    {
        // If there are laned notes, always trick rotation handles into thinking a rotation is in progress
        // This prevents the handle from attempting any continous rotation
        OperationInProgress.Value = selectedHitObjects.Any(s => s is SentakkiLanedHitObject);

        if (OperationInProgress.Value)
            return;

        if (selectedHitObjects.Count == 0)
            return;

        DefaultOrigin = GeometryUtils.GetSurroundingQuad(selectedHitObjects.OfType<IHasPosition>().Select(p => p.Position)).Centre;
    }

    public override void Update(float rotation, Vector2? origin = null)
    {
        if (selectedHitObjects.Any(h => h is SentakkiLanedHitObject))
        {
            laneBasedRotation(rotation);
            return;
        }

        selectionOriginBasedRotation(rotation);
    }

    private void laneBasedRotation(float rotation)
    {
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

    private void selectionOriginBasedRotation(float rotation)
    {
        if (DefaultOrigin is null)
            return;

        foreach (var item in selectedHitObjects.OfType<IHasPosition>())
        {
            var result = GeometryUtils.RotatePointAroundOrigin(originalPosition[item], DefaultOrigin.Value, rotation);

            item.Position = result;

            editorBeatmap.Update((HitObject)item);
        }
    }

    private Dictionary<IHasPosition, Vector2> originalPosition = [];

    public override void Begin()
    {
        base.Begin();

        originalPosition.Clear();

        foreach (var touch in selectedHitObjects.OfType<IHasPosition>())
        {
            originalPosition[touch] = touch.Position;
        }
    }

    public override void Commit()
    {
        // If there are laned notes, always trick rotation handles into thinking a rotation is in progress
        if (selectedHitObjects.Any(s => s is SentakkiLanedHitObject))
            return;

        base.Commit();
    }
}
