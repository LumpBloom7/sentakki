using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Sentakki.UI;

public partial class SentakkiResumeOverlay : DelayedResumeOverlay
{
    private SentakkiCursorContainer? localCursorContainer;
    public override CursorContainer? LocalCursor => State.Value is Visibility.Visible ? localCursorContainer : null;

    public SentakkiResumeOverlay()
    {
        localCursorContainer = new SentakkiCursorContainer();
    }

    protected override void PopIn()
    {
        base.PopIn();
        GameplayCursor?.ActiveCursor.Hide();

        Add(localCursorContainer);
    }

    protected override void PopOut()
    {
        base.PopOut();

        GameplayCursor?.ActiveCursor.Show();

        Remove(localCursorContainer, false);
    }
}
