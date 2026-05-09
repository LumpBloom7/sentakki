using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Sentakki.Edit.Checks;

namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class SentakkiBeatmapVerifier : IBeatmapVerifier
{
    private ICheck[] checks = [
        new CheckSlideTapOmission(),
        new CheckMismatchedSlideTaps(),
    ];

    public IEnumerable<Issue> Run(BeatmapVerifierContext context) => checks.SelectMany(c => c.Run(context));
}
