using osu.Framework.Allocation;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Types;
using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints;

public abstract partial class LanedPlacementBlueprint<T> : SentakkiPlacementBlueprint<T>
    where T : SentakkiHitObject, IHasLane, new()
{
    [Resolved]
    private SentakkiHitObjectComposer composer { get; set; } = null!;

    [Resolved]
    private EditorBeatmap editorBeatmap { get; set; } = null!;

    public override bool ReplacesExistingObject(HitObject existing)
        => base.ReplacesExistingObject(existing)
            && (existing is IHasLane lanedNote)
            && lanedNote.Lane == HitObject.Lane
            && CanReplaceSlide(existing);

    private bool CanReplaceSlide(HitObject existing)
    {
        if (existing is not Slide s)
            return true;

        if (s.TapType is Slide.TapTypeEnum.None)
            return false;

        s.TapType = Slide.TapTypeEnum.None;

        composer.Playfield.Remove(s);
        composer.Playfield.Add(s);

        editorBeatmap.Update(s);

        return false;
    }
}
