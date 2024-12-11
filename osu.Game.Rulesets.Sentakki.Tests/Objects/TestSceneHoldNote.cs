﻿using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    [TestFixture]
    public partial class TestSceneHoldNote : OsuTestScene
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;
        protected override Ruleset CreateRuleset() => new SentakkiRuleset();

        private int depthIndex;

        public TestSceneHoldNote()
        {
            base.Content.Add(content = new SentakkiInputManager(new SentakkiRuleset().RulesetInfo));

            AddStep("Miss Insane Short", () => testSingle(100));
            AddStep("Hit Insane Short", () => testSingle(100, true));
            AddStep("Miss Very Short", () => testSingle(200));
            AddStep("Hit Very Short", () => testSingle(200, true));
            AddStep("Miss Short", () => testSingle(500));
            AddStep("Hit Short", () => testSingle(500, true));
            AddStep("Miss Medium", () => testSingle(750));
            AddStep("Hit Medium", () => testSingle(750, true));
            AddStep("Miss Long", () => testSingle(1000));
            AddStep("Hit Long", () => testSingle(1000, true));
            AddStep("Miss Very Long", () => testSingle(3000));
            AddStep("Hit Very Long", () => testSingle(3000, true));
            AddUntilStep("Wait for object despawn", () => !Children.Any(h => h is DrawableSentakkiHitObject sentakkiHitObject && sentakkiHitObject.AllJudged == false));
        }

        private void testSingle(double duration, bool auto = false)
        {
            var circle = new Hold
            {
                StartTime = Time.Current + 1000,
                EndTime = Time.Current + 1000 + duration,
            };

            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            Add(new DrawableHold(circle)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Depth = depthIndex++,
                Auto = auto
            });
        }
    }
}
