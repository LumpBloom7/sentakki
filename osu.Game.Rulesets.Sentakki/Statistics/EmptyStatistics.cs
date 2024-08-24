using osu.Game.Screens.Ranking.Statistics;

namespace osu.Game.Rulesets.Sentakki.Statistics;

public partial class EmptyStatistics : SimpleStatisticItem<string>
{
    public EmptyStatistics() : base("")
    {
    }

    protected override string DisplayValue(string value)
    {
        return "";
    }
}
