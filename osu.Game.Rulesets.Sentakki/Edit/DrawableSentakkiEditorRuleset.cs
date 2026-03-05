using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class DrawableSentakkiEditorRuleset : DrawableSentakkiRuleset
{
    public DrawableSentakkiEditorRuleset(SentakkiRuleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod>? mods) : base(ruleset, beatmap, mods)
    {
    }

    public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer()
    => new SentakkiPlayfieldAdjustmentContainer() { Size = new Vector2(0.9f) };
}