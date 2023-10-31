using osu.Game.Rulesets.Edit;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit;

public class SentakkiLanedSnapResult : SnapResult
{
    public SentakkiLanedSnapResult(Vector2 screenSpacePosition, int lane, double? time) : base(screenSpacePosition, time, null)
    {
        Lane = lane;
    }

    public int Lane { get; set; }

    public float YPos { get; set; }
}
