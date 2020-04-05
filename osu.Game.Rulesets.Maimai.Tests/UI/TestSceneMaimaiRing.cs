using NUnit.Framework;
using osu.Game.Rulesets.Maimai.UI.Components;
using osu.Framework.Graphics.Shapes;
using osu.Game.Tests.Visual;
using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Maimai.Objects;
using osu.Game.Rulesets.Maimai.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Maimai.Tests.UI
{
    [TestFixture]
    public class TestSceneMaimaiRing : OsuTestScene
    {
        private MaimaiRing ring;
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(MaimaiRing)
        };

        public TestSceneMaimaiRing()
        {
            AddStep("Clear test", () =>
            {
                Clear();
                Add(new Box
                {
                    RelativeSizeAxes = Framework.Graphics.Axes.Both
                });
            });

            AddStep("Create Ring", () => Add(ring = new MaimaiRing()));
            AddUntilStep("Ring loaded", () => ring.IsLoaded && ring.Alpha == 1);
            AddRepeatStep("Flash Ring", () => ring.Flash(), 5);
            AddToggleStep("Toggle notestart Indicators", b => ring.noteStartIndicators.Value = b);
            AddSliderStep<float>("Test opacity", 0, 1, 1, f => { if (ring != null) ring.ringOpacity.Value = f; });
        }
    }
}
