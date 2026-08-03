using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Skinning;
using osu.Game.Rulesets.Sentakki.Skinning.Default;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Tests.UI;

[TestFixture]
public partial class TestSceneSentakkiRing : SentakkiSkinnableTestScene
{
    [Test]
    public void TestPlayfieldRing()
    {
        AddStep("Create Ring", () => SetContents(_ => new SkinnableDrawable(new SentakkiSkinComponentLookup(SentakkiSkinComponents.PlayfieldRing), _ => new PlayfieldRing())
        {
            RelativeSizeAxes = Axes.None,
            Size = new Vector2(SentakkiPlayfield.RINGSIZE),
            Scale = new Vector2(0.3f),
        }));

        AddUntilStep("Ring loaded", () => CreatedDrawables.All(d => d.IsLoaded));
        AddToggleStep("Toggle notestart Indicators", toggleNotestartIndicators);
        AddRepeatStep("Trigger Kiai Beat", triggerKiaiBeat, 5);
        AddSliderStep<float>("Test opacity", 0, 1, 1, adjustOpacity);
    }

    private void toggleNotestartIndicators(bool b)
    {
        CreatedDrawables.OfType<SkinnableDrawable>().Select(d => d.Drawable).OfType<PlayfieldRing>().ForEach(
            r => r.NoteStartIndicators.Value = b
        );
    }

    private void triggerKiaiBeat()
    {
        CreatedDrawables.OfType<SkinnableDrawable>().Select(d => d.Drawable).OfType<PlayfieldRing>().ForEach(
            r => r.Pulse()
        );
    }

    private void adjustOpacity(float f)
    {
        CreatedDrawables.OfType<SkinnableDrawable>().Select(d => d.Drawable).OfType<PlayfieldRing>().ForEach(
            r => r.RingOpacity.Value = f
        );
    }
}
