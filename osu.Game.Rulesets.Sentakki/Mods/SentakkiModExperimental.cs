// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModExperimental : Mod, IApplicableToBeatmapConverter
    {
        public override string Name => "Experimental";
        public override string Description => "Some experimental features to be added to future sentakki builds. Autoplay recommended.";
        public override string Acronym => "Ex";

        public override IconUsage? Icon => FontAwesome.Solid.Flask;
        public override ModType Type => ModType.Fun;

        public override bool Ranked => false;

        public override double ScoreMultiplier => 1.00;

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            (beatmapConverter as SentakkiBeatmapConverter).Experimental = true;
        }
    }
}
