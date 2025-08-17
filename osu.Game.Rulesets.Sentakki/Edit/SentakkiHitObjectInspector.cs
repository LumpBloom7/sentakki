using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class SentakkiHitObjectInspector : HitObjectInspector
{
    protected override void AddInspectorValues(HitObject[] objects)
    {
        base.AddInspectorValues(objects);

        if (objects.Length != 1)
            return;

        SentakkiHitObject senObject = (SentakkiHitObject)objects[0];

        AddHeader("Break");
        AddValue(senObject.Break.ToString());

        if (senObject is not TouchHold)
        {
            AddHeader("Ex");
            AddValue(senObject.Ex.ToString());
        }

        if (senObject is not Slide slide || slide.SlideInfoList.Count == 0)
            return;

        switch (slide.SlideInfoList.Count)
        {
            case 1:
                AddHeader("Shoot offset");
                AddValue($"{slide.SlideInfoList[0].ShootDelay:0.##} beats");

                AddHeader("Break slide");
                AddValue(slide.SlideInfoList[0].Break.ToString());

                AddHeader("Ex slide");
                AddValue(slide.SlideInfoList[0].Ex.ToString());

                AddHeader("Parts");

                foreach (SlideBodyPart part in slide.SlideInfoList[0].SlidePathParts)
                {
                    int simpleEndOffset = part.EndOffset;
                    if (simpleEndOffset > 4)
                        simpleEndOffset -= 8;

                    string mirrored = "";

                    switch (part.Shape)
                    {
                        case SlidePaths.PathShapes.Circle:
                            mirrored = part.Mirrored ? "CCW" : "CW";
                            break;

                        case SlidePaths.PathShapes.U:
                        case SlidePaths.PathShapes.Cup:
                        case SlidePaths.PathShapes.Thunder:
                            mirrored = part.Mirrored ? "M" : "";
                            break;
                    }

                    AddValue($"{part.Shape}({simpleEndOffset}){mirrored}");
                }

                break;

            default:
                AddHeader("Slide bodies");
                AddValue($"{slide.SlideInfoList.Count}");
                break;
        }
    }
}
