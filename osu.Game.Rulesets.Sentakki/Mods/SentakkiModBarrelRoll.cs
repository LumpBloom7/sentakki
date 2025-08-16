using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sentakki.Mods;

public partial class SentakkiModBarrelRoll : ModBarrelRoll<SentakkiHitObject>
{
    public override void Update(Playfield playfield)
    {
        // We only rotate the main playfield
        if (playfield is SentakkiPlayfield)
            base.Update(playfield);
    }
}
