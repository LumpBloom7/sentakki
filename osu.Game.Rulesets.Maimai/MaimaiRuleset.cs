// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Maimai.Scoring;
using osu.Game.Rulesets.Maimai.Beatmaps;
using osu.Game.Rulesets.Maimai.Mods;
using osu.Game.Rulesets.Maimai.UI;
using osu.Game.Rulesets.Maimai.Configuration;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Maimai
{
    public class MaimaiRuleset : Ruleset
    {
        public override string Description => "maimai";

        public override ScoreProcessor CreateScoreProcessor() => new MaimaiScoreProcessor();

        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods) =>
            new DrawableMaimaiRuleset(this, beatmap, mods);

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) =>
            new MaimaiBeatmapConverter(beatmap, this);

        public override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) =>
            new MaimaiDifficultyCalculator(this, beatmap);

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new MultiMod(new MaimaiModHalfTime(), new MaimaiModDaycore()),
                        new MaimaiModNoFail(),
                    };
                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new MaimaiModSuddenDeath(),
                        new MultiMod(new MaimaiModDoubleTime(), new MaimaiModNightcore()),
                    };
                case ModType.Automation:
                    return new[] { new MaimaiModAutoplay() };

                case ModType.Fun:
                    return new Mod[]
                    {
                        new MultiMod(new ModWindUp(), new ModWindDown()),
                    };
                default:
                    return new Mod[] { null };
            }
        }

        public override string ShortName => "Maimai";

        public override RulesetSettingsSubsection CreateSettings() => new MaimaiSettingsSubsection(this);

        public override IRulesetConfigManager CreateConfig(SettingsStore settings) => new MaimaiRulesetConfigManager(settings, RulesetInfo);

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => new[]
        {
            new KeyBinding(InputKey.Z, MaimaiAction.Button1),
            new KeyBinding(InputKey.X, MaimaiAction.Button2),
        };

        public override Drawable CreateIcon() => new Sprite
        {
            Texture = new TextureStore(new TextureLoaderStore(CreateResourceStore()), false).Get("Textures/Icon2"),
        };
    }
}
