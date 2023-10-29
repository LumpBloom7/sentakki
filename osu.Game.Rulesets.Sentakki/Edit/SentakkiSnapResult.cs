using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit;

public class SentakkiSnapResult : SnapResult
{
    public SentakkiSnapResult(Vector2 screenSpacePosition, double? time, Playfield? playfield = null) : base(screenSpacePosition, time, playfield)
    {
    }

    public int Lane { get; set; }

    public float YPos { get; set; }
}
