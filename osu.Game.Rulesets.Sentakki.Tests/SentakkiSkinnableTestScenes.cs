using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests;

public partial class SentakkiSkinnableTestScene : SkinnableTestScene
{
    private Container content = null!;

    protected override Container<Drawable> Content
    {
        get
        {
            if (content == null)
                base.Content.Add(content = new SentakkiInputManager(new SentakkiRuleset().RulesetInfo));

            return content;
        }
    }



    protected override Ruleset CreateRulesetForSkinProvider() => new SentakkiRuleset();
}
