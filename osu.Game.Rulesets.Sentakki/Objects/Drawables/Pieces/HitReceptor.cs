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

        private List<SentakkiAction> actions = new List<SentakkiAction>();
        public Func<bool> Hit;
        public Action Release;

        public float NoteAngle = -1;
        public bool HoverAction()
        {
            if (!SentakkiActionInputManager.CurrentAngles.Contains(NoteAngle))
            {
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
            SentakkiActionInputManager.CurrentAngles.Remove(NoteAngle);
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

        protected override void Update()
        {
            base.Update();
            if (!SentakkiActionInputManager.PressedActions.Any() && actions.Any())
            {
                SentakkiActionInputManager.CurrentAngles.Remove(NoteAngle);
                actions.Clear();
                Release?.Invoke();
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

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            SentakkiActionInputManager.CurrentAngles.Remove(NoteAngle);
        }

        internal class HoverReceptor : CircularContainer
        {
            public HoverReceptor()
            {
                Size = new Vector2(80);
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            protected override bool OnHover(HoverEvent e) => (Parent as HitReceptor).HoverAction();
        }
    }
}
