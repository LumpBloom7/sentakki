using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning
{
    public interface ITouchPiece : IDrawable
    {
        // Shows as soon as Time >= StartTime
        Drawable TouchBorder { get; }
    }
}
