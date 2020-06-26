// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class SentakkiSelectionHandler : SelectionHandler
    {
        public override bool HandleMovement(MoveSelectionEvent moveEvent)
        {
            if (SelectedBlueprints.Count() > 1)
                return true;

            foreach (var h in SelectedHitObjects.OfType<SentakkiHitObject>())
            {
                if (h is TouchHold)
                {
                    // Spinners don't support position adjustments
                    continue;
                }
                var newPos = ToLocalSpace(moveEvent.ScreenSpacePosition);
                newPos.Y = -newPos.Y;
                var path = newPos.GetDegreesFromPosition(new Vector2(300, -300)).GetNotePathFromDegrees();
                var angle = path.GetAngleFromPath();

                if (h is Hold)
                {
                    h.EndPosition = new Vector2(0, -SentakkiPlayfield.INTERSECTDISTANCE + 40);
                }
                else if (h is Tap)
                {
                    h.Position = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.NOTESTARTDISTANCE, angle);
                    h.EndPosition = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, angle);
                }

                h.Angle = angle;
            }
            return true;
        }
    }
}