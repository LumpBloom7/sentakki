using System.Collections.Generic;
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
        private readonly Dictionary<Drawable, DrawableHitObject> drawableMap = new Dictionary<Drawable, DrawableHitObject>();

        public void Add(Drawable slideBodyProxy, DrawableHitObject hitObject)
        {
            drawableMap.Add(slideBodyProxy, hitObject);
            AddInternal(slideBodyProxy);
        }

        protected override int Compare(Drawable x, Drawable y)
        {
            // Put earlier hitobjects towards the end of the list, so they show above the rest
            int i = getStartTimeOf(drawableMap[y]).CompareTo(getStartTimeOf(drawableMap[x]));
            return i == 0 ? base.Compare(x, y) : i;
        }

        private double getStartTimeOf(DrawableHitObject hitObject) => hitObject.StartTimeBindable.Value;

        protected override void Update()
        {
            base.Update();

            // Used to resolve a potential edge case bug that could happen when abusing the animation speed slider or gameplay rewind
            SortInternal();
        }
    }
}
