using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit.Checks;

public class CheckSentakkiConcurrentObjects : CheckConcurrentObjects
{
    public override IEnumerable<Issue> Run(BeatmapVerifierContext context) => checkLaneNotes(context).Concat(checkTouchNotes(context));

    private IEnumerable<Issue> checkTouchNotes(BeatmapVerifierContext context)
    {
        var hitObjects = context.Beatmap.HitObjects.Where(h => h is Touch or TouchHold).ToList();

        for (int i = 0; i < hitObjects.Count - 1; ++i)
        {
            var hitobject = hitObjects[i];

            for (int j = i + 1; j < hitObjects.Count; ++j)
            {
                var nextHitobject = hitObjects[j];

                // We only care if both objects simulatenously occupy the same position
                // TODO: This should also take into account closeness
                if (hitobject.GetPosition() != nextHitobject.GetPosition())
                    continue;

                // Two hitobjects cannot be concurrent without also being concurrent with all objects in between.
                // So if the next object is not concurrent, then we know no future objects will be either.
                if (!AreConcurrent(hitobject, nextHitobject))
                    break;

                if (hitobject.GetType() == nextHitobject.GetType())
                    yield return new IssueTemplateConcurrentSame(this).Create(hitobject, nextHitobject);
                else
                    yield return new IssueTemplateConcurrentDifferent(this).Create(hitobject, nextHitobject);
            }
        }
    }
    private IEnumerable<Issue> checkLaneNotes(BeatmapVerifierContext context)
    {
        var hitObjects = getAllLanedObjects(context.Beatmap.HitObjects).Cast<SentakkiLanedHitObject>().ToList();

        for (int i = 0; i < hitObjects.Count - 1; ++i)
        {
            var hitobject = hitObjects[i];

            for (int j = i + 1; j < hitObjects.Count; ++j)
            {
                var nextHitobject = hitObjects[j];

                // We only care if both objects simulatenously occupy the same lane
                if (hitobject.Lane != nextHitobject.Lane)
                    continue;

                // Two hitobjects cannot be concurrent without also being concurrent with all objects in between.
                // So if the next object is not concurrent, then we know no future objects will be either.
                if (!AreConcurrent(hitobject, nextHitobject))
                    break;

                if (hitobject.GetType() == nextHitobject.GetType())
                    yield return new IssueTemplateConcurrentSame(this).Create(hitobject, nextHitobject);
                else
                    yield return new IssueTemplateConcurrentDifferent(this).Create(hitobject, nextHitobject);
            }
        }
        yield break;
    }

    private IEnumerable<HitObject> getAllLanedObjects(IReadOnlyList<HitObject> hitObjects)
    {
        for (int i = 0; i < hitObjects.Count(); ++i)
        {
            var hitObject = hitObjects[i];
            if (isLanedObject(hitObject))
                yield return hitObject;

            foreach (var nested in getAllLanedObjects(hitObject.NestedHitObjects))
                yield return nested;
        }
    }

    private bool isLanedObject(HitObject hitObject) => hitObject switch
    {
        Tap => true,
        Hold => true,
        _ => false,
    };
}
