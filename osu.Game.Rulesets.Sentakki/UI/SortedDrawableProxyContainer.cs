using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class SortedDrawableProxyContainer : LifetimeManagementContainer
    {
        public SortedDrawableProxyContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        private readonly Dictionary<Drawable, IBindable<double>> startTimeMap = new Dictionary<Drawable, IBindable<double>>();

        public void Add(Drawable proxy, DrawableHitObject hitObject)
        {

            var startTimeBindable = hitObject.StartTimeBindable.GetBoundCopy();
            startTimeBindable.BindValueChanged(s =>
            {
                if (LoadState >= LoadState.Ready)
                    SortInternal();
            });
            startTimeMap.Add(proxy, startTimeBindable);
            AddInternal(proxy);
        }

        protected override int Compare(Drawable x, Drawable y)
        {
            // Put earlier hitobjects towards the end of the list, so they show above the rest
            int i = startTimeMap[y].Value.CompareTo(startTimeMap[x].Value);
            return i == 0 ? base.Compare(x, y) : i;
        }
    }
}
