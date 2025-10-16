using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit.Checks;

public class CheckSentakkiSlideBodyLength : ICheck
{
    private readonly RidiculouslyFastSlideIssueTemplate tooFastSlideTemplate;
    private readonly FastSlideIssueTemplate fastSlideTemplate;
    private readonly SlowSlideIssueTemplate slowSlideTemplate;

    public CheckSentakkiSlideBodyLength()
    {
        tooFastSlideTemplate = new RidiculouslyFastSlideIssueTemplate(this);
        fastSlideTemplate = new FastSlideIssueTemplate(this);
        slowSlideTemplate = new SlowSlideIssueTemplate(this);

        PossibleTemplates = [tooFastSlideTemplate, fastSlideTemplate, slowSlideTemplate];
    }

    public IEnumerable<Issue> Run(BeatmapVerifierContext context)
    {
        var slides = context.CurrentDifficulty.Playable.HitObjects.OfType<Slide>();

        foreach (var slide in slides)
        {
            foreach (var slideBodyInfo in slide.SlideInfoList)
            {
                double travelTime = Math.Max(slideBodyInfo.Duration - slideBodyInfo.ShootDelay, 0);

                if (travelTime <= slideBodyInfo.SlidePath.MinDuration / 2)
                    yield return tooFastSlideTemplate.Create(slide, travelTime, slideBodyInfo.SlidePath.MinDuration);
                else if (travelTime < slideBodyInfo.SlidePath.MinDuration)
                    yield return fastSlideTemplate.Create(slide, travelTime, slideBodyInfo.SlidePath.MinDuration);
                else if (travelTime > slideBodyInfo.SlidePath.MaxDuration)
                    yield return slowSlideTemplate.Create(slide, travelTime, slideBodyInfo.SlidePath.MaxDuration);
            }
        }
    }

    public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.HitObjects, "Slide body length");
    public IEnumerable<IssueTemplate> PossibleTemplates { get; }

    private class RidiculouslyFastSlideIssueTemplate(ICheck check)
        : IssueTemplate(check, IssueType.Problem, $"{nameof(SlideBody)} is ridiculously fast ({{0}}ms). Recommended minimum travel time is {{1}}ms.")
    {
        public Issue Create(HitObject hitObject, double currentTravelTime, double minimumTravelTime) =>
            new Issue(hitObject, this, currentTravelTime.ToString("N0"), minimumTravelTime.ToString("N0"));
    }

    private class FastSlideIssueTemplate(ICheck check)
        : IssueTemplate(check, IssueType.Warning, $"{nameof(SlideBody)} is a bit fast ({{0}}ms). Recommended minimum travel time is {{1}}ms.")
    {
        public Issue Create(HitObject hitObject, double currentTravelTime, double minimumTravelTime) =>
            new Issue(hitObject, this, currentTravelTime.ToString("N0"), minimumTravelTime.ToString("N0"));
    }

    private class SlowSlideIssueTemplate(ICheck check)
        : IssueTemplate(check, IssueType.Warning, $"{nameof(SlideBody)} is slow ({{0}}ms). Recommended maximum travel time is {{1}}ms.")
    {
        public Issue Create(HitObject hitObject, double currentTravelTime, double maximumTravelTime) =>
            new Issue(hitObject, this, currentTravelTime.ToString("N0"), maximumTravelTime.ToString("N0"));
    }
}
