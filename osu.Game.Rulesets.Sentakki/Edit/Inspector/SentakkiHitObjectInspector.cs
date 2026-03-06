using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Edit.Inspector.Slides;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Types;
using osu.Game.Screens.Edit;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Inspector;

public partial class SentakkiHitObjectInspector : CompositeDrawable
{
    protected OsuTextFlowContainer inspectorText = null!;

    [Resolved]
    private EditorBeatmap editorBeatmap { get; set; } = null!;

    [Resolved]
    private OverlayColourProvider colourProvider { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        AutoSizeAxes = Axes.Y;
        RelativeSizeAxes = Axes.X;

        InternalChild = inspectorText = new OsuTextFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        editorBeatmap.SelectedHitObjects.CollectionChanged += (_, _) => updateInspectorText();
        editorBeatmap.PlacementObject.BindValueChanged(_ => updateInspectorText());
        editorBeatmap.TransactionBegan += updateInspectorText;
        editorBeatmap.TransactionEnded += updateInspectorText;
        updateInspectorText();
    }

    private void updateInspectorText()
    {
        inspectorText.Clear();

        HitObject[] objects;

        if (editorBeatmap.SelectedHitObjects.Count > 0)
            objects = [.. editorBeatmap.SelectedHitObjects];
        else if (editorBeatmap.PlacementObject.Value != null)
            objects = [editorBeatmap.PlacementObject.Value];
        else
            objects = [];

        addInspectorValues(objects);
    }

    private void addInspectorValues(HitObject[] objects)
    {
        switch (objects.Length)
        {
            case 0:
                addValue("No selection");
                break;

            // This is an intentional reimplementation of the base behaviour.
            // This is done to add the beat count to the durations.
            case 1:
                SentakkiHitObject selected = (SentakkiHitObject)objects.Single();

                addHeader("Type");
                addValue($"{selected.GetType().ReadableName()}");

                addPositionInformation(selected);

                addModifierInformation(selected);
                addSlideModifiersInformation(selected);

                addHeader("Time");
                addValue($"{selected.StartTime:#,0.##}ms");
                addDurationInformation(selected);

                addSlideSegmentInformation(selected);
                break;

            default:
                addHeader("Selected Objects");
                addValue($"{objects.Length:#,0.##}");

                addHeader("Start Time");
                addValue($"{objects.Min(o => o.StartTime):#,0.##}ms");

                addHeader("End Time");
                addValue(new SelfUpdatingInspectorEntry(() => $"{objects.Max(o => o.GetEndTime()):#,0.##}ms"));
                break;
        }
    }

    private void addPositionInformation(SentakkiHitObject hitObject)
    {
        addHeader("Position");

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

        addHeader("Duration");
        addValue(new SelfUpdatingInspectorEntry(() => $"{duration.Duration:#,0.##}ms"));
        addValue(new SelfUpdatingInspectorEntry(() => $"{duration.Duration / beatLength:0.##} beats"));

        if (hitObject is not Slide s)
            return;

        double waitDurationInBeats = s.SlideInfoList[0].EffectiveWaitDuration / beatLength;
        double movementDurationInBeats = s.SlideInfoList[0].EffectiveMovementDuration / beatLength;

        addHeader("Wait duration");
        addValue(new SelfUpdatingInspectorEntry(() => $"{s.SlideInfoList[0].EffectiveWaitDuration:#,0.##}ms"));
        addValue(new SelfUpdatingInspectorEntry(() => $"{s.SlideInfoList[0].EffectiveWaitDuration / beatLength:0.##} beats"));

        addHeader("Movement duration");
        addValue(new SelfUpdatingInspectorEntry(() => $"{s.SlideInfoList[0].EffectiveMovementDuration:#,0.##}ms"));
        addValue(new SelfUpdatingInspectorEntry(() => $"{s.SlideInfoList[0].EffectiveMovementDuration / beatLength:0.##} beats"));
    }

    private void addModifierInformation(SentakkiHitObject hitObject)
    {
        addHeader("Modifiers");

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

        addHeader("Slide modifiers");

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
            addHeader("Slide bodies");
            addValue($"{s.SlideInfoList.Count}");
        }

        addHeader("Segments");
        addValue(new SlideBodyInspectorSection(s, s.SlideInfoList[0]));
    }

    private void addHeader(string header) => inspectorText.AddParagraph($"{header}: ", s =>
    {
        s.Font = OsuFont.Style.Caption1;
        s.Colour = colourProvider.Content2;
    });

    private void addValue<T>(T value) where T : Drawable
    {
        inspectorText.NewLine();
        inspectorText.AddArbitraryDrawable(value);
    }

    // This is an alternative implementation that reduces the spacing between the values and the headers
    private void addValue(string value) => addValue(value, colourProvider.Content1);
    private void addValue(string value, Color4 colour)
    {
        inspectorText.NewLine();
        inspectorText.AddText(value, s =>
        {
            s.Font = OsuFont.Style.Body;
            s.Colour = colour;
        });
    }
}
