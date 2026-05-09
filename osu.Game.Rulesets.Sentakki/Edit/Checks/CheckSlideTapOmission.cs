
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit.Checks;

public class CheckSlideTapOmission : ICheck
{
    public CheckMetadata Metadata => new CheckMetadata(CheckCategory.HitObjects, "Incorrectly omitted slides", CheckScope.Difficulty);

    public IEnumerable<IssueTemplate> PossibleTemplates => throw new System.NotImplementedException();

    public CheckSlideTapOmission()
    {
        omittedSlideTapIssueTemplate = new(this);
    }

    private OmittedSlideTapIssueTemplate omittedSlideTapIssueTemplate;

    public IEnumerable<Issue> Run(BeatmapVerifierContext context)
    {
        var slides = context.CurrentDifficulty.Playable.HitObjects.OfType<Slide>();
        foreach (var slide in slides)
        {
            if (slide.TapType is Slide.TapTypeEnum.None)
                yield return omittedSlideTapIssueTemplate.Create(slide);
        }
    }

    public class OmittedSlideTapIssueTemplate(ICheck check)
        : IssueTemplate(check, IssueType.Warning, $"{nameof(Slide)} omits initial tap. This will slightly alter {nameof(SlideBody)} input behaviour.")
    {
        public Issue Create(Slide slide) => new Issue(slide, this);
    }
}
