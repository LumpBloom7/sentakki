// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Maimai.Objects;
using osu.Game.Rulesets.Maimai.Objects.Drawables;
using osuTK;
using System.Collections.Generic;
using System;
using osu.Game.Rulesets.Mods;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Maimai.Tests.Objects
{
    [TestFixture]
    public class TestSceneTapNote : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DrawableMaimaiHitObject)
        };

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private int depthIndex;

        public TestSceneTapNote()
        {
            base.Content.Add(content = new MaimaiInputManager(new RulesetInfo { ID = 0 }));

            //Child = testSingle(2);
            AddStep("Miss Single", () => testSingle());
            AddStep("Hit Single", () => testSingle(true));

        }

        private void testSingle(bool auto = false)
        {

            var circle = new MaimaiHitObject
            {
                StartTime = Time.Current + 1000,
                Position = new Vector2(0, -66f),
                Angle = 0,
                endPosition = new Vector2(0, -296.5f),
                NoteColor = Color4.Orange,
            };

            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { });

            var drawable = CreateDrawableTapNote(circle, auto);
            //foreach (var mod in SelectedMods.Value.OfType<IApplicableToDrawableHitObjects>())
            //    mod.ApplyToDrawableHitObjects(new[] { drawable });

            Add(drawable);
        }

        protected virtual TestDrawableTapNote CreateDrawableTapNote(MaimaiHitObject circle, bool auto) => new TestDrawableTapNote(circle, auto)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Depth = depthIndex++,
        };

        protected class TestDrawableTapNote : DrawableMaimaiHitObject
        {
            private readonly bool auto;

            public TestDrawableTapNote(MaimaiHitObject h, bool auto)
                : base(h)
            {
                this.auto = auto;
            }

            public void TriggerJudgement() => UpdateResult(true);

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                if (auto && !userTriggered && timeOffset > 0)
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
