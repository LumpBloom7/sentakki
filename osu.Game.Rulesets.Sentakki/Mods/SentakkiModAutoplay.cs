﻿using System;
using System.Collections.Generic;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Replays;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModAutoplay : ModAutoplay, IApplicableToDrawableHitObject
    {
        private string getRandomCharacter() => RNG.NextBool() ? "Mai-chan" : "Sen-kun";

        public override Score CreateReplayScore(IBeatmap beatmap, IReadOnlyList<Mod> mods) => new Score
        {
            ScoreInfo = new ScoreInfo { User = new User { Username = getRandomCharacter() } },
            Replay = new SentakkiAutoGenerator(beatmap).Generate(),
        };

        public override Type[] IncompatibleMods => new Type[6]
        {
            typeof(ModRelax),
            typeof(ModSuddenDeath),
            typeof(ModPerfect),
            typeof(ModNoFail),
            typeof(SentakkiModAutoTouch),
            typeof(SentakkiModChallenge)
        };

        public void ApplyToDrawableHitObject(DrawableHitObject drawableHitObject)
        {
            if (!(drawableHitObject is DrawableSentakkiHitObject drawableSentakkiHitObject)) return;

            drawableSentakkiHitObject.Auto = true;
        }
    }
}
