// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Game.Screens.Play;
using static osu.Game.Input.Handlers.ReplayInputHandler;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModRelax : ModRelax, IUpdatableByPlayfield, IApplicableToDrawableRuleset<SentakkiHitObject>, IApplicableToPlayer
    {
        public override string Description => @"You don't need to click. Give your clicking/tapping fingers a break from the heat of things.";

        /// <summary>
        /// How early before a hitobject's start time to trigger a hit.
        /// </summary>
        private const float relax_leniency = 3;

        private bool isDownState;
        private bool wasLeft;

        private bool hasReplay;

        private SentakkiInputManager sentakkiInputManager;

        private ReplayState<SentakkiAction> state;
        private double lastStateChangeTime;

        public void ApplyToDrawableRuleset(DrawableRuleset<SentakkiHitObject> drawableRuleset)
        {
            // grab the input manager for future use.
            sentakkiInputManager = (SentakkiInputManager)drawableRuleset.KeyBindingInputManager;
        }

        public void ApplyToPlayer(Player player)
        {
            if (sentakkiInputManager.ReplayInputHandler != null)
            {
                hasReplay = true;
                return;
            }
            sentakkiInputManager.AllowUserPresses = false;
        }

        public void Update(Playfield playfield)
        {
            if (hasReplay)
                return;

            bool requiresHold = false;
            bool requiresHit = false;

            double time = playfield.Clock.CurrentTime;

            foreach (var h in playfield.HitObjectContainer.AliveObjects.OfType<DrawableSentakkiHitObject>())
            {
                // we are not yet close enough to the object.
                if (time < h.HitObject.StartTime - relax_leniency)
                    break;

                // already hit or beyond the hittable end time.
                if (h.IsHit || (h.HitObject is IHasEndTime hasEnd && time > hasEnd.EndTime))
                    continue;

                switch (h)
                {
                    case DrawableTap tap:
                        if (tap.HitArea.IsHovered)
                        {
                            Debug.Assert(tap.HitObject.HitWindows != null);
                            requiresHit |= tap.HitObject.HitWindows.CanBeHit(time - tap.HitObject.StartTime);
                        }
                        break;

                    case DrawableHold hold:
                        requiresHold |= hold.HitArea.IsHovered || h.IsHovered;
                        break;

                    case DrawableTouchHold _:
                        requiresHold = true;
                        break;
                }
            }

            if (requiresHit)
            {
                changeState(false);
                changeState(true);
            }

            if (requiresHold)
                changeState(true);
            else if (isDownState && time - lastStateChangeTime > AutoGenerator.KEY_UP_DELAY)
                changeState(false);

            void changeState(bool down)
            {
                if (isDownState == down)
                    return;

                isDownState = down;
                lastStateChangeTime = time;

                state = new ReplayState<SentakkiAction>
                {
                    PressedActions = new List<SentakkiAction>()
                };

                if (down)
                {
                    state.PressedActions.Add(wasLeft ? SentakkiAction.Button1 : SentakkiAction.Button2);
                    wasLeft = !wasLeft;
                }

                state?.Apply(sentakkiInputManager.CurrentState, sentakkiInputManager);
            }
        }
    }
}
