using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Types;

namespace osu.Game.Rulesets.Sentakki.Objects;

public abstract class SentakkiLanedHitObject : SentakkiHitObject, IHasLane
{
    private HitObjectProperty<int> lane;

    public Bindable<int> LaneBindable => lane.Bindable;

    public int Lane
    {
        get => lane.Value;
        set => lane.Value = value;
    }
}
