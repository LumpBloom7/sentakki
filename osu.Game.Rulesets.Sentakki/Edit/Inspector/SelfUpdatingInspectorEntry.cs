using System;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Rulesets.Sentakki.Edit.Inspector;

public partial class SelfUpdatingInspectorEntry : OsuSpriteText
{
    private Func<LocalisableString> textUpdateAction;

    [Resolved]
    private OverlayColourProvider colourProvider { get; set; } = null!;

    public SelfUpdatingInspectorEntry(Func<LocalisableString> textUpdateAction)
    {
        this.textUpdateAction = textUpdateAction;
        Font = OsuFont.Style.Body;
        Text = textUpdateAction.Invoke();
    }

    private ScheduledDelegate? rollingTextUpdate;

    protected override void LoadComplete()
    {
        base.LoadComplete();
        Colour = colourProvider.Content1;
        rollingTextUpdate ??= Scheduler.AddDelayed(() => Text = textUpdateAction.Invoke(), 250, true);
    }
}
