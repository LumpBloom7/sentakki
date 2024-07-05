using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Sentakki.Objects;

public interface IBreakNote
{
    virtual bool NeedBreakSample => true;

    virtual int BreakScoreWeighting => 5;

    BindableBool BreakBindable { get; }

    bool Break { get; set; }
}
