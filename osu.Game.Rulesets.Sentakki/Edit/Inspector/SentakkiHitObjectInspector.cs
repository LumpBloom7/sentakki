using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Types;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Inspector;

public partial class SentakkiHitObjectInspector : EditorInspector
{
    [Resolved]
    private EditorBeatmap editorBeatmap { get; set; } = null!;

    [Resolved]
    private OverlayColourProvider colourProvider { get; set; } = null!;

    protected override void LoadComplete()
    {
        base.LoadComplete();

        EditorBeatmap.SelectedHitObjects.CollectionChanged += (_, _) => updateInspectorText();
        EditorBeatmap.PlacementObject.BindValueChanged(_ => updateInspectorText());
        EditorBeatmap.TransactionBegan += updateInspectorText;
        EditorBeatmap.TransactionEnded += updateInspectorText;
        updateInspectorText();
    }


    private void updateInspectorText()
    {
        InspectorText.Clear();

        HitObject[] objects;

        if (EditorBeatmap.SelectedHitObjects.Count > 0)
            objects = EditorBeatmap.SelectedHitObjects.ToArray();
        else if (EditorBeatmap.PlacementObject.Value != null)
            objects = [EditorBeatmap.PlacementObject.Value];
        else
            objects = [];

        AddInspectorValues(objects);
    }


    protected void AddInspectorValues(HitObject[] objects)
    {
        switch (objects.Length)
        {
            default:
            case 0:
                AddValue("No selection");
                break;

            // This is an intentional reimplementation of the base behaviour.
            // This is done to add the beat count to the durations.
            case 1:
                SentakkiHitObject selected = (SentakkiHitObject)objects.Single();

                AddHeader("Type");
                addValue($"{selected.GetType().ReadableName()}");

                addPositionInformation(selected);

                addModifierInformation(selected);
                addSlideModifiersInformation(selected);

                AddHeader("Time");
                addValue($"{selected.StartTime:#,0.##}ms");

                addDurationInformation(selected);

                addSlideSegmentInformation(selected);
                break;
        }
    }

    private void addPositionInformation(SentakkiHitObject hitObject)
    {
        AddHeader("Position");

        switch (hitObject)
        {
            case IHasPosition pos:
                addValue($"x:{pos.X:#,0.##}");
                addValue($"y:{pos.Y:#,0.##}");
                break;

            case IHasLane lane:
                addValue($"Lane: {lane.Lane}");
                break;
        }
    }

    private void addDurationInformation(SentakkiHitObject hitObject)
    {
        if (hitObject is not IHasDuration duration)
            return;

        double beatLength = editorBeatmap.ControlPointInfo.TimingPointAt(hitObject.StartTime).BeatLength;
        double durationInBeats = duration.Duration / beatLength;

        AddHeader("Duration");
        addValue($"{duration.Duration:#,0.##}ms");
        addValue($"{durationInBeats:0.##} beats");

        if (hitObject is not Slide s)
            return;

        double waitDurationInBeats = s.SlideInfoList[0].EffectiveWaitDuration / beatLength;
        double movementDurationInBeats = s.SlideInfoList[0].EffectiveMovementDuration / beatLength;

        AddHeader("Wait duration");
        addValue($"{s.SlideInfoList[0].EffectiveWaitDuration:#,0.##}ms");
        addValue($"{waitDurationInBeats:0.##} beats");

        AddHeader("Movement duration");
        addValue($"{s.SlideInfoList[0].EffectiveMovementDuration:#,0.##}ms");
        addValue($"{movementDurationInBeats:0.##} beats");
    }

    private void addModifierInformation(SentakkiHitObject hitObject)
    {
        AddHeader("Modifiers");

        List<string> modifiers = [];

        if (hitObject.Break)
            modifiers.Add("Break");

        if (hitObject.Ex)
            modifiers.Add("Ex");

        if (modifiers.Count == 0)
        {
            addValue("None");
            return;
        }

        addValue(string.Join(", ", [.. modifiers]));
    }

    private void addSlideModifiersInformation(SentakkiHitObject hitObject)
    {
        if (hitObject is not Slide s || s.SlideInfoList.Count != 1)
            return;

        AddHeader("Slide modifiers");

        List<string> modifiers = [];

        if (s.SlideInfoList[0].Break)
            modifiers.Add("Break");

        if (s.SlideInfoList[0].Ex)
            modifiers.Add("Ex");

        if (modifiers.Count == 0)
        {
            addValue("None");
            return;
        }

        addValue(string.Join(", ", [.. modifiers]));
    }

    private void addSlideSegmentInformation(SentakkiHitObject hitObject)
    {
        if (hitObject is not Slide s)
            return;

        if (s.SlideInfoList.Count != 1)
        {
            AddHeader("Slide bodies");
            addValue($"{s.SlideInfoList.Count}");
        }

        AddHeader("Segments");
        for (int i = 0; i < s.SlideInfoList[0].Segments.Count; ++i)
        {
            InspectorText.NewLine();
            InspectorText.AddArbitraryDrawable(new SentakkiSlideSegmentInspectorEntry(s, s.SlideInfoList[0], i));
        }
    }


    // This is an alternative implementation that reduces the spacing between the values and the headers
    private void addValue(string value) => addValue(value, colourProvider.Content1);
    private void addValue(string value, Color4 colour)
    {
        InspectorText.NewLine();
        InspectorText.AddText(value, s =>
        {
            s.Font = s.Font.With(weight: FontWeight.SemiBold);
            s.Colour = colour;
        });
    }
}