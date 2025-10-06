using System;
using System.Collections.Generic;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Replays;

namespace osu.Game.Rulesets.Sentakki.Mods;

public class SentakkiModAutoplay : ModAutoplay, IApplicableToDrawableHitObject
{
    private static string getRandomCharacter() => RNG.NextBool() ? "Mai-chan" : "Sen-kun";

    public override ModReplayData CreateReplayData(IBeatmap beatmap, IReadOnlyList<Mod> mods)
        => new ModReplayData(new SentakkiAutoGenerator(beatmap).Generate(), new ModCreatedUser { Username = getRandomCharacter() });

    public override Type[] IncompatibleMods =>
    [
        typeof(ModRelax),
        typeof(ModFailCondition),
        typeof(SentakkiModNoTouch)
    ];

    public void ApplyToDrawableHitObject(DrawableHitObject drawableHitObject)
    {
        if (drawableHitObject is not DrawableSentakkiHitObject drawableSentakkiHitObject) return;

        drawableSentakkiHitObject.Auto = true;
    }
}
