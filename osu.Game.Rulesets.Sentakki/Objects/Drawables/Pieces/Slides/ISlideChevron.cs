namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides
{
    public interface ISlideChevron
    {
        // The SlideBody completion threshold that causes this chevron to disappear
        public double DisappearThreshold { get; set; }

        public float Alpha { get; set; }

        public SlideVisual? SlideVisual { get; set; }

        public static void UpdateProgress(ISlideChevron chevron)
        {
            chevron.Alpha = chevron.SlideVisual?.Progress >= chevron.DisappearThreshold ? 0 : 1;
        }
    }
}
