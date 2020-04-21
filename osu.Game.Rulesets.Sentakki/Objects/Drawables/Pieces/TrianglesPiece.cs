using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class TrianglesPiece : Triangles
    {
        protected override float SpawnRatio => 1f;

        public TrianglesPiece()
        {
            TriangleScale = 1.2f;
            HideAlphaDiscrepancies = false;
        }

        protected override void Update()
        {
            if (IsPresent)
                base.Update();
        }
    }
}
