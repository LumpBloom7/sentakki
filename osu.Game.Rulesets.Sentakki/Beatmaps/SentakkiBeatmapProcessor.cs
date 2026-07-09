using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Beatmaps;

public class SentakkiBeatmapProcessor : BeatmapProcessor
{
    public new SentakkiBeatmap Beatmap => (SentakkiBeatmap)(base.Beatmap is EditorBeatmap eb ? eb.PlayableBeatmap : base.Beatmap);

    public Action<SentakkiBeatmap>? CustomNoteColouringDelegate = null;

    public SentakkiBeatmapProcessor(IBeatmap beatmap)
        : base(beatmap)
    {
    }

    public override void PostProcess()
    {
        base.PostProcess();

        if (CustomNoteColouringDelegate is not null)
            CustomNoteColouringDelegate?.Invoke(Beatmap);
        else
            applyDefaultNoteColouring();
    }

    private void applyDefaultNoteColouring()
    {
        Color4 twinColor = Color4.Gold;
        Color4 breakColor = Color4.OrangeRed;

        var colourableHitObjects = getColorableHitObject(Beatmap.HitObjects)
            .GroupByDictionary(h =>
                {
                    if (h is SlideBody sb)
                        return (true, Math.Round(sb.StartTime + sb.SlideBodyInfo.EffectiveWaitDuration));

                    return (false, Math.Round(h.StartTime));
                }
            );

        foreach (var group in colourableHitObjects.Values)
        {
            bool isTwin = group.Count(countsForTwin) > 1; // This determines whether the twin colour should be used for eligible objects

            foreach (SentakkiHitObject hitObject in group)
            {
                if (hitObject is TouchHold th)
                {
                    th.ColourPalette = TouchHold.DEFAULT_PALETTE;

                    if (th.Break)
                        th.ColourPalette = TouchHold.BREAK_PALETTE;
                    else if (isTwin)
                        th.ColourPalette = TouchHold.TWIN_PALETTE;

                    continue;
                }

                Color4 noteColor = hitObject.DefaultNoteColour;

                if (hitObject.Break)
                    noteColor = breakColor;
                else if (isTwin)
                    noteColor = twinColor;

                hitObject.NoteColour = noteColor;
            }
        }
    }

    private static IEnumerable<SentakkiHitObject> getColorableHitObject(List<SentakkiHitObject> hitObjects)
    {
        foreach (var hitObject in hitObjects)
        {
            yield return hitObject;

            switch (hitObject)
            {
                case Hold h:
                    // The HitExplosion uses the colour of the hold head as well as the hold itself.
                    yield return (Hold.HoldHead)h.NestedHitObjects[0];
                    break;

                case Slide s:
                    if (s.TapType is not Slide.TapTypeEnum.None)
                        yield return s.SlideTap;

                    foreach (var slideBody in s.SlideBodies)
                        yield return slideBody;

                    break;
            }
        }
    }

    private static bool countsForTwin(HitObject hitObject) => hitObject switch
    {
        Hold.HoldHead => false,
        Slide => false,
        _ => true
    };
}
