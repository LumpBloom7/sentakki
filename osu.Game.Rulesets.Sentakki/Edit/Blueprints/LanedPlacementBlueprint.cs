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
    {
        bool isCandidateForRemoval = base.ReplacesExistingObject(existing)
                                        && (existing is IHasLane lanedNote)
                                        && lanedNote.Lane == HitObject.Lane;

        if (!isCandidateForRemoval)
            return false;

        // If the existing object is a slide, we need to perform special handling to avoid the entire slide being deleted
        //   when the better (and more sensible) option is to simply remove the slide-tap.
        if (existing is not Slide s)
            return true;

        // If the slide has a tap associated with it. We will remove the tap note associated with it.
        if (s.TapType is not Slide.TapTypeEnum.None)
        {
            s.TapType = Slide.TapTypeEnum.None;

            composer.Playfield.Remove(s);
            composer.Playfield.Add(s);

            editorBeatmap.Update(s);
        }

        return false;
    }
}
