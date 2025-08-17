using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Sentakki.UI;

public partial class SortedDrawableProxyContainer : LifetimeManagementContainer
{
    public SortedDrawableProxyContainer()
    {
        RelativeSizeAxes = Axes.Both;
    }

    private readonly Dictionary<Drawable, IBindable<double>> startTimeMap = [];

    // We use this to batch changes
    private bool sortQueued = true;

    public void Add(Drawable proxy, DrawableHitObject hitObject)
    {
        var startTimeBindable = hitObject.StartTimeBindable.GetBoundCopy();
        startTimeBindable.BindValueChanged(onStartTimeChange);
        startTimeMap.Add(proxy, startTimeBindable);
        AddInternal(proxy);
    }

    private void onStartTimeChange(ValueChangedEvent<double> v)
    {
        if (LoadState >= LoadState.Ready)
            sortQueued = true;
    }

    protected override void Update()
    {
        base.Update();

        if (!sortQueued) return;

        SortInternal();
        sortQueued = false;
    }

    protected override int Compare(Drawable x, Drawable y)
    {
        // Put earlier hitobjects towards the end of the list, so they show above the rest
        int i = startTimeMap[y].Value.CompareTo(startTimeMap[x].Value);
        return i == 0 ? base.Compare(x, y) : i;
    }
}
