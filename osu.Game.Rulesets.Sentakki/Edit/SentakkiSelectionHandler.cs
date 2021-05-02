using osu.Game.Extensions;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class SentakkiSelectionHandler : EditorSelectionHandler
    {
        public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent)
        {
            if (SelectedBlueprints.Count > 1)
                return false;

            switch (moveEvent.Blueprint.Item)
            {
                case Touch t:
                    Vector2 HitObjectPosition = t.Position;
                    HitObjectPosition += this.ScreenSpaceDeltaToParentSpace(moveEvent.ScreenSpaceDelta);

                    if (Vector2.Distance(Vector2.Zero, HitObjectPosition) > 250)
                    {
                        var currentAngle = Vector2.Zero.GetDegreesFromPosition(HitObjectPosition);
                        HitObjectPosition = SentakkiExtensions.GetCircularPosition(250, currentAngle);
                    }

                    t.Position = HitObjectPosition;
                    break;
            }
            return true;
        }
    }
}
