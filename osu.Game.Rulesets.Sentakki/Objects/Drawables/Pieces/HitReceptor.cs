using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;
using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class HitReceptor : CircularContainer, IKeyBindingHandler<SentakkiAction>
    {
        // IsHovered is used
        public override bool HandlePositionalInput => true;

        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

        private List<SentakkiAction> actions = new List<SentakkiAction>();
        public Func<bool> Hit;
        public Action Release;

        public float NoteAngle = -1;
        public bool HoverAction()
        {
            if (!SentakkiActionInputManager.CurrentAngles.Contains(NoteAngle))
            {
                SentakkiActionInputManager.CurrentAngles.Add(NoteAngle);
                if (SentakkiActionInputManager.PressedActions.Any(action => OnPressed(action)))
                    actions.AddRange(SentakkiActionInputManager.PressedActions);
            }
            return false;
        }
        public HitReceptor()
        {
            RelativeSizeAxes = Axes.None;
            Size = new Vector2(240);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Add(new HoverReceptor());
        }

        public virtual bool OnPressed(SentakkiAction action)
        {
            switch (action)
            {
                default:
                    if (IsHovered && (Hit?.Invoke() ?? false))
                    {
                        actions.Add(action);
                        return true;
                    }
                    break;
            }
            return false;
        }

        public void OnReleased(SentakkiAction action)
        {
            switch (action)
            {
                default:
                    if (actions.Contains(action))
                        actions.Remove(action);
                    if (!actions.Any())
                        Release?.Invoke();
                    break;
            }
        }
        protected override void OnHoverLost(HoverLostEvent e)
        {
            SentakkiActionInputManager.CurrentAngles.Remove(NoteAngle);
            actions.Clear();
            Release?.Invoke();
        }

        internal class HoverReceptor : CircularContainer
        {
            public HoverReceptor()
            {
                Size = new Vector2(150);
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            protected override bool OnHover(HoverEvent e) => (Parent as HitReceptor).HoverAction();
        }
    }
}