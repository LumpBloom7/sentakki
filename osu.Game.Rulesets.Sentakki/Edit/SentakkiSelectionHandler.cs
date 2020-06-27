// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System;
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
                var newPos = ToLocalSpace(moveEvent.ScreenSpacePosition);
                newPos = new Vector2(newPos.X - 300, -(newPos.Y - 300));

                switch (h)
                {
                    case TouchHold _:
                        continue;
                    case Touch touch:
                    {
                        float angle = newPos.GetDegreesFromPosition(Vector2.Zero);
                        float distance = Math.Clamp(Vector2.Distance(newPos, Vector2.Zero), 0, 200);
                        newPos = SentakkiExtensions.GetCircularPosition(distance, angle);

                        touch.Position = newPos;
                        break;
                    }
                    case Tap _:
                    {
                        var path = newPos.GetDegreesFromPosition(Vector2.Zero).GetNotePathFromDegrees();
                        var angle = path.GetAngleFromPath();
                        h.Position = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.NOTESTARTDISTANCE, angle);
                        h.EndPosition = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, angle);
                        h.Angle = angle;
                        break;
                    }
                    case Hold _:
                    {
                        var path = newPos.GetDegreesFromPosition(Vector2.Zero).GetNotePathFromDegrees();
                        var angle = path.GetAngleFromPath();
                        h.EndPosition = new Vector2(0, -SentakkiPlayfield.INTERSECTDISTANCE + 40);
                        h.Angle = angle;
                        break;
                    }
                }
            }
            return true;
        }
    }
}