using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;

namespace osu.Game.Rulesets.Objects.Drawables
{
    public interface IDrawableSlideBody
    {
        public Container<DrawableSlideCheckpoint> SlideCheckpoints { get; }
    }
}
