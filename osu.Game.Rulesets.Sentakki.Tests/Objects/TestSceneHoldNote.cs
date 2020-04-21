// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    [TestFixture]
    public class TestSceneHoldNote : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DrawableHold)
        };

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private int depthIndex;

        public TestSceneHoldNote()
        {
            base.Content.Add(content = new SentakkiInputManager(new RulesetInfo { ID = 0 }));

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
        }

        private void testSingle(double duration, bool auto = false)
        {
            var circle = new Hold
            {
                StartTime = Time.Current + 1000,
                EndTime = Time.Current + 1000 + duration,
                Position = new Vector2(0, -66),
                endPosition = new Vector2(0, -296.5f),
                Angle = 0f,
                NoteColor = Color4.Crimson,
            };

            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { });

            var drawable = new TestDrawableHoldNote(circle, auto);

            Add(drawable);
        }

        protected virtual TestDrawableHoldNote CreateDrawableHoldNote(Hold circle, bool auto) => new TestDrawableHoldNote(circle, auto)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Depth = depthIndex++,
        };

        protected class TestDrawableHoldNote : DrawableHold
        {
            public TestDrawableHoldNote(Hold h, bool auto)
                : base(h)
            {
                this.Auto = auto;
            }

            public void TriggerJudgement() => UpdateResult(true);

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                if (this.Auto && !userTriggered && timeOffset > 0)
                {
                    // force success
                    ApplyResult(r => r.Type = HitResult.Perfect);
                }
                else
                    base.CheckForResult(userTriggered, timeOffset);
            }
        }
    }
}
