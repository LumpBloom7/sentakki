using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osuTK;
using System;
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

        private readonly List<SentakkiAction> actions = new List<SentakkiAction>();
        public Func<bool> Hit;
        public Action Release;

        public float NoteAngle = -1;
        public bool HoverAction()
        {
            if (!SentakkiActionInputManager.CurrentAngles.Contains(NoteAngle))
            {
                if (SentakkiActionInputManager.PressedActions.Any(action => OnPressed(action)))
                    actions.AddRange(SentakkiActionInputManager.PressedActions.Except(actions));
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
            if (IsHovered)
            {
                SentakkiActionInputManager.CurrentAngles.Add(NoteAngle);
                if (Hit?.Invoke() ?? false)
                {
                    actions.Add(action);
                    return true;
                }
            }
            return false;
        }

        public void OnReleased(SentakkiAction action)
        {
            actions.Remove(action);
            if (!actions.Any())
            {
                Release?.Invoke();
                SentakkiActionInputManager.CurrentAngles.Remove(NoteAngle);
            }
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            SentakkiActionInputManager.CurrentAngles.Remove(NoteAngle);
            if (actions.Any())
            {
                actions.Clear();
                Release?.Invoke();
            }
        }

        internal class HoverReceptor : CircularContainer
        {
            public HoverReceptor()
            {
                Size = new Vector2(40);
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            protected override bool OnHover(HoverEvent e) => (Parent as HitReceptor).HoverAction();
        }
    }
}
