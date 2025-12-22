using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
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
                AddValue($"{selected.GetType().ReadableName()}");

                addPositionInformation(selected);

                addModifierInformation(selected);
                addSlideModifiersInformation(selected);

                AddHeader("Time");
                AddValue($"{selected.StartTime:#,0.##}ms");

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
                AddValue($"x:{pos.X:#,0.##}");
                AddValue($"y:{pos.Y:#,0.##}");
                break;

            case IHasLane lane:
                AddValue($"Lane: {lane.Lane}");
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

        addValueWithSubvalue(
                $"{duration.Duration:#,0.##}ms",
                $"{durationInBeats:0.##} beats",
                colourProvider.Content1);

        if (hitObject is not Slide s)
            return;

        double waitDurationInBeats = s.SlideInfoList[0].EffectiveWaitDuration / beatLength;
        double movementDurationInBeats = s.SlideInfoList[0].EffectiveMovementDuration / beatLength;

        AddHeader("Wait duration");

        addValueWithSubvalue(
            $"{s.SlideInfoList[0].EffectiveWaitDuration:#,0.##}ms",
            $"{waitDurationInBeats:0.##} beats",
            colourProvider.Content1
        );

        AddHeader("Movement duration");
        addValueWithSubvalue(
            $"{s.SlideInfoList[0].EffectiveMovementDuration:#,0.##}ms",
            $"{movementDurationInBeats:0.##} beats",
            colourProvider.Content1
        );
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
            AddValue("None");
            return;
        }

        AddValue(string.Join(", ", [.. modifiers]));
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
            AddValue("None");
            return;
        }

        AddValue(string.Join(", ", [.. modifiers]));
    }

    private void addSlideSegmentInformation(SentakkiHitObject hitObject)
    {
        if (hitObject is not Slide s)
            return;

        if (s.SlideInfoList.Count != 1)
        {
            AddHeader("Slide bodies");
            AddValue($"{s.SlideInfoList.Count}");
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

            AddValue($"{segment.Shape}({simpleEndOffset}){mirrored}");
        }
    }

    private void addValueWithSubvalue(string value, string subvalue, Color4 colour, Color4? subvalueColour = null)
    {
        InspectorText.AddParagraph("");

        var wrappingContainer = new FillFlowContainer()
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Vertical,
            Children = [
                new OsuSpriteText()
                {
                    Text = value,
                    Font = OsuFont.Torus.With(weight: FontWeight.SemiBold),
                    Colour = colour,
                },
                new OsuSpriteText()
                {
                    Text = subvalue,
                    Margin = new MarginPadding { Bottom = -5, },
                    Font = OsuFont.Torus.With(size: 12, weight: FontWeight.SemiBold),
                    Colour = subvalueColour ?? colour,
                }
            ]
        };

        InspectorText.AddArbitraryDrawable(wrappingContainer);
    }
}