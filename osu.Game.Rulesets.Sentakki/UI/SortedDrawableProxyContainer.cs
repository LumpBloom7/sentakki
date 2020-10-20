using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class SortedDrawableProxyContainer : LifetimeManagementContainer
    {
        public SortedDrawableProxyContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        private readonly Dictionary<Drawable, int> drawableMap = new Dictionary<Drawable, int>();

        public void Add(Drawable slideBodyProxy)
        {
            drawableMap.Add(slideBodyProxy, InternalChildren.Count);
            AddInternal(slideBodyProxy);
        }

        protected override int Compare(Drawable x, Drawable y)
        {
            // Put earlier hitobjects towards the end of the list, so they show above the rest
            int i = drawableMap[y].CompareTo(drawableMap[x]);
            return i == 0 ? CompareReverseChildID(x, y) : i;
        }
    }
}
