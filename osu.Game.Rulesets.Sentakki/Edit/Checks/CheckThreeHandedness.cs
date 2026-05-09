using System.Collections.Generic;
using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit.Checks;

public class CheckThreeHandedness : ICheck
{
    public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Compose, "Too many hands needed", CheckScope.Difficulty);

    public IEnumerable<IssueTemplate> PossibleTemplates { get; }

    private IssueTemplateTooManyHands issueTemplateTooManyHands;

    public CheckThreeHandedness()
    {
        PossibleTemplates = [
            issueTemplateTooManyHands = new IssueTemplateTooManyHands(this)
        ];
    }

    public IEnumerable<Issue> Run(BeatmapVerifierContext context)
    {
        var periods = createPeriods(context.CurrentDifficulty.Playable.HitObjects.OfType<SentakkiLanedHitObject>())
                                        .SelectMany(p => new PeriodEvent[]{
                                            new PeriodEvent(p.Start, false, p),
                                            new PeriodEvent(p.End, true, p)
                                        })
                                        .OrderBy(p => p.time)
                                        .ThenBy(p => p.end ? 1 : 0)
                                        .ToArray();

        HashSet<Period> overlapping = [];

        bool justRemoved = true;

        foreach (var period in periods)
        {
            if (!period.end)
            {
                overlapping.Add(period.period);
                justRemoved = false;
                continue;
            }

            if (period.end)
            {
                int numberOfObjects = overlapping.GroupBy(p => (p.Mergeable, ((SentakkiLanedHitObject)p.hitObject).Lane))
                                                 .Sum(g => g.Key.Mergeable ? 1 : g.Count());

                if (numberOfObjects > 2 && !justRemoved)
                {
                    yield return issueTemplateTooManyHands.Create(period.time, overlapping.Select(p => p.hitObject));
                    justRemoved = true;
                }

                overlapping.Remove(period.period);
            }
        }
    }

    public class IssueTemplateTooManyHands(ICheck check)
    : IssueTemplate(check, IssueType.Problem, "More than 2 hands needed to play")
    {
        public Issue Create(IEnumerable<HitObject> hitObjects) => new Issue(hitObjects, this);
        public Issue Create(double time, IEnumerable<HitObject> hitObjects) => new Issue(hitObjects, this)
        {
            Time = time
        };
    }

    private static IEnumerable<Period> createPeriods(IEnumerable<HitObject> hitObjects)
    {
        foreach (var hitObject in hitObjects)
        {
            switch (hitObject)
            {
                case Slide s:
                    if (s.TapType is not Slide.TapTypeEnum.None)
                        yield return new Period(s, s.StartTime, s.StartTime);

                    foreach (var slideInfo in s.SlideInfoList)
                        yield return new Period(s, s.StartTime + slideInfo.EffectiveWaitDuration, s.StartTime + slideInfo.Duration, false);

                    break;
                default:
                    yield return new Period(hitObject, hitObject.StartTime, hitObject.GetEndTime(), false);
                    break;
            }
        }
    }

    private record struct PeriodEvent(double time, bool end, Period period);

    private record Period(HitObject hitObject, double Start, double End, bool Mergeable = true)
    {
        // We guarantee that the objects are either treated as concurrent or unsnapped when near the same beat divisor.
        private const double ms_leniency = CheckUnsnappedObjects.UNSNAP_MS_THRESHOLD;

        public bool IsOverlapped(Period other)
        {
            return Start.CompareTo(other.End) <= 0 && other.Start.CompareTo(End) <= 0;
        }
    }
}
