namespace osu.Game.Rulesets.Sentakki.Skinning.Common;

// Used mainly in cases were we want to be able to copy visual state from identical drawables (such as in the editor)
// Without exposing too much internal implementation details
public interface IHasCopyableVisualState
{
    void CopyVisualStateTo(IHasCopyableVisualState other);
}
