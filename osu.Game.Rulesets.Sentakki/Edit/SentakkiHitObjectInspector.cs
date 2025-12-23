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
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Rulesets.Sentakki.Objects.Types;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class SentakkiHitObjectInspector : HitObjectInspector
{
    [Resolved]
    private EditorBeatmap editorBeatmap { get; set; } = null!;

    [Resolved]
    private OverlayColourProvider colourProvider { get; set; } = null!;

    protected override void AddInspectorValues(HitObject[] objects)
    {
        switch (objects.Length)
        {
            default:
            case 0:
                base.AddInspectorValues(objects);
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
        foreach (var segment in s.SlideInfoList[0].Segments)
        {
            int simpleEndOffset = segment.RelativeEndLane;
            if (simpleEndOffset > 4)
                simpleEndOffset -= 8;

            string mirrored = "";

            switch (segment.Shape)
            {
                case PathShape.Circle:
                    mirrored = segment.Mirrored ? "CCW" : "CW";
                    break;

                case PathShape.U:
                case PathShape.Cup:
                case PathShape.Thunder:
                    mirrored = segment.Mirrored ? "M" : "";
                    break;
            }

            addValue($"{segment.Shape}({simpleEndOffset}){mirrored}");
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