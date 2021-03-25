namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class SentakkiSlideInfo
    {
        // Index of the slide, used to select a slide pattern from the list of ValidPaths
        public int ID;
        public bool Mirrored;

        // Duration of the slide
        public double Duration;

        // Delay before the star on the slide starts moving to the end
        public int ShootDelay = 1;

        public SentakkiSlidePath SlidePath => SlidePaths.GetSlidePath(ID, Mirrored);
    }
}
