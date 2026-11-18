using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.HUD;

public abstract partial class ContextualCounter : CompositeDrawable, ISerialisableDrawable
{
    public bool UsesFixedAnchor { get; set; }

    [SettingSource("Default display")]
    public Bindable<DefaultDisplay> DefaultDisplayMode { get; } = new();

    [SettingSource("Accuracy display mode")]
    public Bindable<GameplayAccuracyCounter.AccuracyDisplayMode> AccuracyDisplayMode { get; } = new();

    protected ContextualCounter()
    {
        AutoSizeAxes = Axes.Both;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        applyOverrideCounterFromMods();
        DefaultDisplayMode.BindValueChanged(displayModeChanged, true);
    }

    private void recreateCounter()
    {
        InternalChild = CreateComponentFor(DefaultDisplayMode.Value);
    }

    protected abstract Drawable CreateComponentFor(DefaultDisplay mode);

    protected abstract Drawable? CreateOverrideCounterFor(IProvideContextualCounter mod);

    private void displayModeChanged(ValueChangedEvent<DefaultDisplay> defaultDisplay)
    {
        if (overrideCounterBindable is not null)
            return;

        recreateCounter();
    }

    [Resolved]

    private Player player { get; set; } = null!;

    private IBindable? overrideCounterBindable { get; set; }

    private void applyOverrideCounterFromMods()
    {
        var mod = player.GameplayState.Mods.OfType<IProvideContextualCounter>().SingleOrDefault();

        if (mod is null)
            return;

        var counterBindable = mod.CounterBindable;

        var overrideCounter = CreateOverrideCounterFor(mod);

        if (overrideCounter is null)
            return;

        InternalChild = overrideCounter;
        overrideCounterBindable = counterBindable;
    }

    public enum DefaultDisplay
    {
        Combo,
        Accuracy,
        Score,
    }
}
