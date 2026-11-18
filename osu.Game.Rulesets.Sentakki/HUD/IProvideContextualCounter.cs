using osu.Framework.Bindables;
using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.HUD;

public interface IProvideContextualCounter
{
    LocalisableString CounterLabel { get; }
    IBindable CounterBindable { get; }
}
