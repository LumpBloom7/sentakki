using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class SlideFan : SlideBody
    {
        protected override void CreateSlideCheckpoints()
        {
            // Add body nodes (should be two major sets)
            Vector2 originpoint = new Vector2(0, -SentakkiPlayfield.INTERSECTDISTANCE);
            for (int i = 1; i < 5; ++i)
            {
                float progress = 0.25f * i;
                SlideCheckpoint checkpoint = new SlideCheckpoint()
                {
                    Progress = progress,
                    StartTime = StartTime + ShootDelay + ((Duration - ShootDelay) * progress),
                    NodesToPass = 2,
                };

                for (int j = 3; j < 6; ++j)
                {
                    Vector2 dest = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, j.GetRotationForLane() - 22.5f);
                    checkpoint.NodePositions.Add(Vector2.Lerp(originpoint, dest, progress));
                }
                AddNested(checkpoint);
            }
        }
    }
}
