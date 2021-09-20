using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Tests.UI
{
    [TestFixture]
    public class TestSceneSentakkiRing : OsuTestScene
    {
        private SentakkiRing ring;

        public TestSceneSentakkiRing()
        {
            AddStep("Clear test", () =>
            {
                Clear();
                Add(new Box
                {
                    RelativeSizeAxes = Framework.Graphics.Axes.Both
                });
            });

            AddStep("Create Ring", () => Add(ring = new SentakkiRing()
            {
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(SentakkiPlayfield.RINGSIZE)
            }));

            AddUntilStep("Ring loaded", () => ring.IsLoaded && ring.Alpha == 1);
            AddToggleStep("Toggle notestart Indicators", b => ring.NoteStartIndicators.Value = b);
            AddRepeatStep("Trigger Kiai Beat", () => ring.KiaiBeat(), 5);
            AddSliderStep<float>("Test opacity", 0, 1, 1, f => { if (ring != null) ring.RingOpacity.Value = f; });
        }
    }
}
