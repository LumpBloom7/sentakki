namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides
{
    public interface ISlideChevron
    {
        public double Progress { get; set; }

        public float Alpha { get; set; }

        public static void UpdateProgress(ISlideChevron chevron, double progress)
        {
            chevron.Alpha = progress >= chevron.Progress ? 0 : 1;
        }
    }
}
