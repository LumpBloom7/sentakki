using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit.Checks;

public class CheckSlideShapes : ICheck
{
    private readonly StraightVSlideIssueTemplate straightVSlideTemplate;
    private readonly Straight1SlideIssueTemplate straight1SlideTemplate;
    private readonly OnlyShort1SlideIssueTemplate onlyShort1SlideTemplate;
    private readonly SequentialCircularSlidesIssueTemplate sequentialCircularSlidesTemplate;
    private readonly HiddenCircularReversalSlidesIssueTemplate hiddenCircularReversalSlidesTemplate;

    public CheckSlideShapes()
    {
        PossibleTemplates =
        [
            straight1SlideTemplate = new Straight1SlideIssueTemplate(this),
            straightVSlideTemplate = new StraightVSlideIssueTemplate(this),
            onlyShort1SlideTemplate = new OnlyShort1SlideIssueTemplate(this),
            sequentialCircularSlidesTemplate = new SequentialCircularSlidesIssueTemplate(this),
            hiddenCircularReversalSlidesTemplate = new HiddenCircularReversalSlidesIssueTemplate(this),
        ];
    }

    public IEnumerable<Issue> Run(BeatmapVerifierContext context)
    {
        var slides = context.Beatmap.HitObjects.OfType<Slide>();

        foreach (var slide in slides)
        {
            foreach (var slideBodyInfo in slide.SlideInfoList)
            {
                if (hasStraightVSlide(slideBodyInfo))
                    yield return straightVSlideTemplate.Create(slide);

                if (hasStraight1Slide(slideBodyInfo))
                    yield return straight1SlideTemplate.Create(slide);

                if (hasOnlyShort1Slide(slideBodyInfo))
                    yield return onlyShort1SlideTemplate.Create(slide);

                if (hasSequentialCircularSlides(slideBodyInfo))
                    yield return sequentialCircularSlidesTemplate.Create(slide);

                if (hasHiddenCircularReversal(slideBodyInfo))
                    yield return hiddenCircularReversalSlidesTemplate.Create(slide);
            }
        }
    }

    private bool hasStraightVSlide(SlideBodyInfo sbi)
        => sbi.SlidePathParts.Any(part => part.Shape is SlidePaths.PathShapes.V && part.EndOffset.NormalizePath() == 4);

    private bool hasStraight1Slide(SlideBodyInfo sbi)
        => sbi.SlidePathParts.Any(part => part.Shape is SlidePaths.PathShapes.Straight && part.EndOffset.NormalizePath() == 1);

    private bool hasOnlyShort1Slide(SlideBodyInfo sbi)
    {
        if (sbi.SlidePathParts.Length > 1)
            return false;

        ref var part = ref sbi.SlidePathParts[0];

        if (part.Shape is not (SlidePaths.PathShapes.Straight or SlidePaths.PathShapes.Circle))
            return false;

        int endOffset = part.EndOffset.NormalizePath();

        if (part.Shape is SlidePaths.PathShapes.Circle && part.Mirrored)
            endOffset = 8 - endOffset;

        return endOffset == 1;
    }

    private bool hasSequentialCircularSlides(SlideBodyInfo sbi)
    {
        for (int i = 1; i < sbi.SlidePathParts.Length; ++i)
        {
            ref var currentPart = ref sbi.SlidePathParts[i];
            ref var previousPart = ref sbi.SlidePathParts[i - 1];

            if (currentPart.Shape is not SlidePaths.PathShapes.Circle)
                continue;

            if (previousPart.Shape is not SlidePaths.PathShapes.Circle)
                continue;

            // We ignore full circles.
            if (previousPart.EndOffset.NormalizePath() == 0 || currentPart.EndOffset.NormalizePath() == 0)
                continue;

            if (currentPart.Mirrored == previousPart.Mirrored)
                return true;
        }

        return false;
    }

    private bool hasHiddenCircularReversal(SlideBodyInfo sbi)
    {
        int reversals = 0;

        for (int i = 1; i < sbi.SlidePathParts.Length; ++i)
        {
            ref var currentPart = ref sbi.SlidePathParts[i];
            ref var previousPart = ref sbi.SlidePathParts[i - 1];

            if (currentPart.Shape is not SlidePaths.PathShapes.Circle
                || previousPart.Shape is not SlidePaths.PathShapes.Circle)
            {
                reversals = 0;
                continue;
            }

            if (currentPart.Mirrored == previousPart.Mirrored)
                continue;

            if (++reversals >= 2)
                return true;
        }

        return false;
    }

    public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.HitObjects, "Odd slide shapes");
    public IEnumerable<IssueTemplate> PossibleTemplates { get; }

    private class SlideIssueTemplate(ICheck check, string unformattedText) : IssueTemplate(check, IssueType.Warning, unformattedText)
    {
        public Issue Create(HitObject ho) => new Issue(ho, this);
    }

    private class StraightVSlideIssueTemplate(ICheck check) :
        SlideIssueTemplate(check, $"{nameof(SlideBody)} contains straight V slide. Consider replacing with a Straight slide instead.");

    private class Straight1SlideIssueTemplate(ICheck check) :
        SlideIssueTemplate(check, $"{nameof(SlideBody)} contains straight +/- 1 slide. These are unsupported in other Maimai implementations.");

    private class OnlyShort1SlideIssueTemplate(ICheck check) :
        SlideIssueTemplate(check, $"{nameof(SlideBody)} only has a short +/-1 slide. These are intended to be part of a larger slide chain.");

    private class SequentialCircularSlidesIssueTemplate(ICheck check) :
        SlideIssueTemplate(check, $"{nameof(SlideBody)} contains multiple sequential circular segments. Consider using a longer circle slide instead.");

    private class HiddenCircularReversalSlidesIssueTemplate(ICheck check) :
        SlideIssueTemplate(check, $"{nameof(SlideBody)} contains potentially hidden circular reversals.");
}
