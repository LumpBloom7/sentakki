using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit.Checks;

public class CheckSlideTapOmission : ICheck
{
    public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.HitObjects, "Incorrectly omitted slides", CheckScope.Difficulty);

    public IEnumerable<IssueTemplate> PossibleTemplates { get; }

    public CheckSlideTapOmission()
    {
        PossibleTemplates = [
            omittedSlideTapIssueTemplate = new(this)
        ];
    }

    private OmittedSlideTapIssueTemplate omittedSlideTapIssueTemplate;

    public IEnumerable<Issue> Run(BeatmapVerifierContext context)
    {
        var hitobjects = context.CurrentDifficulty.Playable.HitObjects.Where(h => h is Tap or Slide).Cast<SentakkiLanedHitObject>().ToArray();

        // Just flag any that have an omitted slide tap
        foreach (var hitobject in hitobjects)
        {
            if (hitobject is not Slide s)
                continue;

            if (s.TapType is Slide.TapTypeEnum.None)
                yield return omittedSlideTapIssueTemplate.Create(s);
        }
    }

    public class OmittedSlideTapIssueTemplate(ICheck check)
        : IssueTemplate(check, IssueType.Warning, $"{nameof(Slide)} omits initial tap. This will slightly alter {nameof(SlideBody)} input behaviour.")
    {
        public Issue Create(Slide slide) => new Issue(slide, this);
    }
}
