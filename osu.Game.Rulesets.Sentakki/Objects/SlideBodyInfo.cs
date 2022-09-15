using osu.Game.Rulesets.Sentakki.Objects.SlidePath;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class SlideBodyInfo
    {
        private PathParameters[] pathParameters;

        public PathParameters[] PathParameters
        {
            get => pathParameters;
            set
            {
                pathParameters = value;
                updatePaths();
            }
        }

        public SentakkiSlidePath SlidePath { get; private set; }

        // Duration of the slide
        public double Duration;

        // Delay before the star on the slide starts moving to the end
        public int ShootDelay = 1;

        private void updatePaths()
        {
            SlidePath = SlidePaths.CreateSlidePath(pathParameters);
        }
    }
}
