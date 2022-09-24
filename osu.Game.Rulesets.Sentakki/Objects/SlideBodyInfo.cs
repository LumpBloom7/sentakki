namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class SlideBodyInfo
    {
        private SlideBodyPart[] pathParameters;

        public SlideBodyPart[] PathParameters
        {
            get => pathParameters;
            set
            {
                pathParameters = value;
                UpdatePaths();
            }
        }

        public SentakkiSlidePath SlidePath { get; private set; }

        // Duration of the slide
        public double Duration;

        // Delay before the star on the slide starts moving to the end
        public int ShootDelay = 1;

        public void UpdatePaths() => SlidePath = SlidePaths.CreateSlidePath(pathParameters);
    }
}
