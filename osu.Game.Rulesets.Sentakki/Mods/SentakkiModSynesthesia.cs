using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Screens.Edit;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Mods;

/// <summary>
/// Mod that colours <see cref="HitObject"/>s based on the musical division they are on
/// </summary>
public class SentakkiModSynesthesia : ModSynesthesia, IApplicableToBeatmapProcessor
{
    [SettingSource("Colour Note Groups")]
    public BindableBool ColourNoteGroups { get; } = new BindableBool(false);

    public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
    {
        if (beatmapProcessor is not SentakkiBeatmapProcessor sbp)
            return;

        sbp.ColouringMode = ColourNoteGroups.Value ? SentakkiBeatmapProcessor.NoteColouringMode.GC_BASED : SentakkiBeatmapProcessor.NoteColouringMode.DIVISOR_BASED;
    }
}

