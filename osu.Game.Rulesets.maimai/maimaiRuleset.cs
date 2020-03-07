// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.maimai.Beatmaps;
using osu.Game.Rulesets.maimai.Mods;
using osu.Game.Rulesets.maimai.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.maimai
{
    public class maimaiRuleset : Ruleset
    {
        public maimaiRuleset(RulesetInfo rulesetInfo = null)
            : base(rulesetInfo)
        {
        }

        public override string Description => "Washing machine rhythm game.";

        public override DrawableRuleset CreateDrawableRulesetWith(IWorkingBeatmap beatmap, IReadOnlyList<Mod> mods) =>
            new DrawablemaimaiRuleset(this, beatmap, mods);

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) =>
            new maimaiBeatmapConverter(beatmap);

        public override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) =>
            new maimaiDifficultyCalculator(this, beatmap);

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.Automation:
                    return new[] { new maimaiModAutoplay() };

                default:
                    return new Mod[] { null };
            }
        }

        public override string ShortName => "maimai";

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => new[]
        {
            new KeyBinding(InputKey.Z, maimaiAction.Button1),
            new KeyBinding(InputKey.X, maimaiAction.Button2),
        };

        public override Drawable CreateIcon() => new Sprite
        {
            Texture = new TextureStore(new TextureLoaderStore(CreateResourceStore()), false).Get("Textures/coin"),
        };
    }
}
