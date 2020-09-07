using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Replays;
using osu.Game.Screens.Play;
using static osu.Game.Input.Handlers.ReplayInputHandler;
using osuTK;
using Microsoft.EntityFrameworkCore.Internal;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class Lane : Playfield
    {
        public int LaneNumber { get; set; }

        public Lane()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.None;
            AddRangeInternal(new Drawable[]{
                HitObjectContainer,
                new LaneReceptor()
            });
        }

        public class LaneReceptor : CompositeDrawable
        {
            private SentakkiInputManager sentakkiActionInputManager;
            internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

            public override bool HandlePositionalInput => true;
            public LaneReceptor()
            {
                Position = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, 0);
                Size = new Vector2(240);

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                CornerRadius = 120;
                CornerExponent = 2;
            }

            protected override void Update()
            {
                base.Update();
                ReplayState<SentakkiAction> state = new ReplayState<SentakkiAction>()
                {
                    PressedActions = SentakkiActionInputManager.PressedActions.ToList()
                };

                if (IsHovered && SentakkiActionInputManager.PressedActions.Any(x => x < SentakkiAction.Key1))
                {
                    if (!state.PressedActions.Contains(SentakkiAction.Key1 + ((Lane)Parent).LaneNumber))
                        state.PressedActions.Add(SentakkiAction.Key1 + ((Lane)Parent).LaneNumber);
                }
                else
                {
                    if (state.PressedActions.Contains(SentakkiAction.Key1 + ((Lane)Parent).LaneNumber))
                        state.PressedActions.RemoveAll(x => x == SentakkiAction.Key1 + ((Lane)Parent).LaneNumber);
                }

                state.Apply(SentakkiActionInputManager.CurrentState, SentakkiActionInputManager);
            }
        }
    }
}
