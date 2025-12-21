using osu.Framework.Allocation;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class SentakkiHitObjectInspector : HitObjectInspector
{
    [Resolved]
    private EditorBeatmap editorBeatmap { get; set; } = null!;

    protected override void AddInspectorValues(HitObject[] objects)
    {
        base.AddInspectorValues(objects);

        if (objects.Length != 1)
            return;

        SentakkiHitObject hitObject = (SentakkiHitObject)objects[0];

        if (hitObject is IHasDuration duration)
        {
            double beatLength = editorBeatmap.ControlPointInfo.TimingPointAt(hitObject.StartTime).BeatLength;
            double durationInBeats = duration.Duration / beatLength;
            AddValue($"{durationInBeats:0.##} beats");
        }

        {
            if (hitObject is Slide s && s.SlideInfoList.Count == 1)
            {
                double beatLength = editorBeatmap.ControlPointInfo.TimingPointAt(hitObject.StartTime).BeatLength;
                double waitDurationInBeats = s.SlideInfoList[0].EffectiveWaitDuration / beatLength;
                double movementDurationInBeats = s.SlideInfoList[0].EffectiveMovementDuration / beatLength;

                AddHeader("Wait duration");
                AddValue($"{s.SlideInfoList[0].EffectiveWaitDuration:#,0.##}ms");
                AddValue($"{waitDurationInBeats:0.##} beats");

                AddHeader("Movement duration");
                AddValue($"{s.SlideInfoList[0].EffectiveMovementDuration:#,0.##}ms");
                AddValue($"{movementDurationInBeats:0.##} beats");
            }
        }

        bool isBreak = hitObject.Break;
        bool isEX = hitObject is not TouchHold && hitObject.Ex;

        if (isBreak || isEX)
        {
            AddHeader("Modifiers");

            if (isBreak)
                AddValue("Break");

            if (isEX)
                AddValue($"EX");
        }

        {
            if (hitObject is Slide s)
            {
                if (s.SlideInfoList.Count != 1)
                {
                    AddHeader("Slide bodies");
                    AddValue($"{s.SlideInfoList.Count}");
                }
                else
                {
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
            }
        }

    }
}