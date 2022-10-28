using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning
{
    public interface ITouchHoldPiece : IDrawable
    {
        // Used to fill the progress bar after HitObject start time
        double Progress { get; set; }
    }
}
