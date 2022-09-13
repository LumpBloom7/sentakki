namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides
{
    public interface ISlideVisual
    {
        public double Progress { get; set; }
        public void PerformEntryAnimation(double duration);
        public void PerformExitAnimation(double duration);

        public void Free();
    }
}
