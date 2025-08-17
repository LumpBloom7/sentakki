using System;
using System.Collections;
using Newtonsoft.Json;

namespace osu.Game.Rulesets.Sentakki.Objects;

public class SlideBodyInfo : IEquatable<SlideBodyInfo>
{
    [JsonIgnore]
    private static readonly SentakkiSlidePath empty_path = SlidePaths.CreateSlidePath(new SlideBodyPart(SlidePaths.PathShapes.Straight, endOffset: 0, false));

    private SlideBodyPart[] slidePathParts = null!;

    public SlideBodyPart[] SlidePathParts
    {
        get => slidePathParts;
        set
        {
            slidePathParts = value;
            UpdatePaths();
        }
    }

    [JsonIgnore]
    public SentakkiSlidePath SlidePath { get; private set; } = empty_path;

    // Duration of the slide
    public double Duration;

    // Delay before the star on the slide starts moving to the end
    // Measured in beats
    public float ShootDelay = 1;

    // Whether the slide body should have a break modifier applied to them.
    public bool Break;

    // Whether the slide body should have the EX modifier applied to them.
    public bool Ex;

    public void UpdatePaths() => SlidePath = SlidePaths.CreateSlidePath(slidePathParts);

    public override bool Equals(object? obj) => obj is SlideBodyInfo other && Equals(other);

    public bool Equals(SlideBodyInfo? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Break != other.Break)
            return false;

        if (Ex != other.Ex)
            return false;

        if (Duration != other.Duration)
            return false;

        if (ShootDelay != other.ShootDelay)
            return false;

        return ShapeEquals(other);
    }

    public bool ShapeEquals(SlideBodyInfo other)
    {
        if (slidePathParts.Length != other.slidePathParts.Length)
            return false;

        for (int i = 0; i < slidePathParts.Length; ++i)
        {
            if (!slidePathParts[i].Equals(other.slidePathParts[i]))
                return false;
        }

        return true;
    }

    public override int GetHashCode()
        => HashCode.Combine(Duration, ShootDelay, StructuralComparisons.StructuralEqualityComparer.GetHashCode(SlidePathParts));
}
