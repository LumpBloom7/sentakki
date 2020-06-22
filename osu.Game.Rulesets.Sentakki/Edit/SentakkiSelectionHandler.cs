using System.Linq;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class SentakkiSelectionHandler : SelectionHandler
    {
        public override bool HandleMovement(MoveSelectionEvent moveEvent)
        {
            Vector2 minPosition = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxPosition = new Vector2(float.MinValue, float.MinValue);

            // Go through all hitobjects to make sure they would remain in the bounds of the editor after movement, before any movement is attempted
            foreach (var h in SelectedHitObjects.OfType<SentakkiHitObject>())
            {
                if (h is TouchHold)
                {
                    // Spinners don't support position adjustments
                    continue;
                }

            }

            if (minPosition.X < 0 || minPosition.Y < 0 || maxPosition.X > DrawWidth || maxPosition.Y > DrawHeight)
                return false;

            foreach (var h in SelectedHitObjects.OfType<SentakkiHitObject>())
            {
                if (h is TouchHold)
                {
                    // Spinners don't support position adjustments
                    continue;
                }

                h.Position += moveEvent.InstantDelta;
            }

            return true;
        }
    }
}