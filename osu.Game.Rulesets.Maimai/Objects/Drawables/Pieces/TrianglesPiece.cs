using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Rulesets.Maimai.Objects.Drawables.Pieces
{
    class TrianglesPiece : Triangles
    {
        protected override float SpawnRatio => 0.5f;

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
