using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSentakkiPool<T> : DrawablePool<T>
        where T : DrawableHitObject, new()
    {
        private readonly Action<Drawable> onLoaded;

        public DrawableSentakkiPool(Action<Drawable> onLoaded, int initialSize, int? maximumSize = null)
            : base(initialSize, maximumSize)
        {
            this.onLoaded = onLoaded;
        }

        protected override T CreateNewDrawable() => base.CreateNewDrawable().With(o =>
        {
            if (o is DrawableSentakkiHitObject senObject)
                senObject.OnLoadComplete += onLoaded;
        });
    }
}
