using osu.Framework.Allocation;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default;

public partial class SlideFanChevron : SlideChevron
{
    public SlideFanChevron(int index = 0)
    {

    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Chevron.FanChevron = true;
    }
}
