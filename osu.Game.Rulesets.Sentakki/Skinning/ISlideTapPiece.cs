using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Sentakki.Skinning
{
    public interface ISlideTapPiece : IDrawable
    {
        public Container Stars { get; }
        public Drawable SecondStar { get; }
    }
}
