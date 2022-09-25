using System;
using System.Collections;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class SlideBodyInfo : IEquatable<SlideBodyInfo>
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

        public override bool Equals(object obj) => obj is SlideBodyInfo other && Equals(other);

        public bool Equals(SlideBodyInfo other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (Duration != other.Duration)
                return false;

            if (ShootDelay != other.ShootDelay)
                return false;

            if (pathParameters.Length != other.pathParameters.Length)
                return false;

            for (int i = 0; i < pathParameters.Length; ++i)
                if (!pathParameters[i].Equals(other.pathParameters[i]))
                    return false;

            return true;
        }

        public override int GetHashCode()
            => HashCode.Combine(Duration, ShootDelay, StructuralComparisons.StructuralEqualityComparer.GetHashCode(PathParameters));
    }
}
