using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Sentakki.Extensions;

public static class BindableExtensions
{
    /// <summary>
    /// Attempts to bind <paramref name="bindable"/> to <paramref name="other"/>, but does nothing if <paramref name="other"/> is null.
    /// </summary>
    public static void TryBindTo<T>(this Bindable<T> bindable, Bindable<T>? other)
    {
        if (other is null)
            return;

        bindable.BindTo(other);
    }
}
