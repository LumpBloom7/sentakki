using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit.Checks;

public class CheckMismatchedSlideTaps : ICheck
{
    public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.HitObjects, "Mismatched Slide tap types", CheckScope.Difficulty);

    public IEnumerable<IssueTemplate> PossibleTemplates { get; }

    public CheckMismatchedSlideTaps()
    {
        PossibleTemplates = [
            mismatchedTapTypeIssueTemplate = new(this)
        ];
    }

    private MismatchedTapTypeIssueTemplate mismatchedTapTypeIssueTemplate;

    public IEnumerable<Issue> Run(BeatmapVerifierContext context)
    {
        var hitobjects = context.CurrentDifficulty.Playable.HitObjects.OfType<Slide>().ToArray();
        List<List<Slide>> concurrentSlides = [];

        foreach (var laneGroup in hitobjects.OfType<Slide>().GroupBy(s => s.Lane))
        {
            if (laneGroup.Count() <= 1)
                continue;

            List<Slide> concurrentGroup = [];

            foreach (var slide in laneGroup)
            {
                if (concurrentGroup.Count == 0)
                {
                    concurrentGroup.Add(slide);
                    continue;
                }

                var previous = concurrentGroup[^1];

                if (!areConcurrent(previous, slide))
                {
                    if (concurrentGroup.Count > 1)
                        concurrentSlides.Add(concurrentGroup);

                    concurrentGroup = [slide];
                    continue;
                }

                concurrentGroup.Add(slide);
            }

            if (concurrentGroup.Count > 1)
                concurrentSlides.Add(concurrentGroup);
        }

        foreach (var concurrentGroup in concurrentSlides)
        {
            bool mismatched = false;
            Slide.TapTypeEnum? tapType = null;

            foreach (var s in concurrentGroup)
            {
                tapType ??= s.TapType;

                mismatched = tapType != s.TapType;
                tapType = tapType.Value < s.TapType ? tapType : s.TapType;
            }

            if (mismatched)
                yield return mismatchedTapTypeIssueTemplate.Create(concurrentGroup, tapType!.Value);
        }
    }


    // We guarantee that the objects are either treated as concurrent or unsnapped when near the same beat divisor.
    private const double ms_leniency = CheckUnsnappedObjects.UNSNAP_MS_THRESHOLD;

    private static bool areConcurrent(HitObject hitobject, HitObject nextHitobject) => nextHitobject.StartTime <= hitobject.GetEndTime() + ms_leniency;

    private class MismatchedTapTypeIssueTemplate(ICheck check)
    : IssueTemplate(check, IssueType.Error, $"Concurrent {nameof(Slide)}s have mismatched tap types. A {{0}} will be used during gameplay.")
    {
        public Issue Create(IEnumerable<Slide> slides, Slide.TapTypeEnum tapType) => new Issue(slides, this, tapType);
    }
}
