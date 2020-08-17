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

        private SentakkiLanedHitObject hitObject = null;
        public bool HoverAction()
        {
            if (hitObject is null || !SentakkiActionInputManager.CurrentPath.Contains(hitObject.Lane))
            {
                if (SentakkiActionInputManager.PressedActions.Any(action => OnPressed(action)))
                    actions.AddRange(SentakkiActionInputManager.PressedActions.Except(actions));
            }
            return false;
        }
        public HitReceptor(SentakkiLanedHitObject hitObject = null)
        {
            this.hitObject = hitObject;
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
                if (hitObject != null)
                    SentakkiActionInputManager.CurrentPath.Add(hitObject.Lane);
                actions.Add(action);
                return Hit?.Invoke() ?? false;
            }
            return false;
        }

        public void OnReleased(SentakkiAction action)
        {
            actions.Remove(action);
            if (!actions.Any())
            {
                Release?.Invoke();
                if (hitObject != null)
                    SentakkiActionInputManager.CurrentPath.Remove(hitObject.Lane);
            }
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (hitObject != null)
                SentakkiActionInputManager.CurrentPath.Remove(hitObject.Lane);
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
                Size = new Vector2(60);
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            protected override bool OnHover(HoverEvent e) => (Parent as HitReceptor).HoverAction();
        }
    }
}
