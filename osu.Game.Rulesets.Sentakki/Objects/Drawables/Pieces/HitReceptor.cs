using osu.Framework.Bindables;
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
    public class HitReceptor : CircularContainer
    {
        // IsHovered is used
        public override bool HandlePositionalInput => true;

        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

        private readonly List<SentakkiAction> actions = new List<SentakkiAction>();
        public Func<bool> Hit;
        public HitReceptor()
        {
            RelativeSizeAxes = Axes.None;
            Size = new Vector2(300);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            trackedKeys.BindValueChanged(x =>
            {
                if (x.NewValue > x.OldValue)
                    Hit?.Invoke();
            });
        }
        private BindableInt trackedKeys = new BindableInt(0);

        protected override void Update()
        {
            base.Update();
            int count = 0;
            var touchInput = SentakkiActionInputManager.CurrentState.Touch;

            if (touchInput.ActiveSources.Any())
                count = touchInput.ActiveSources.Where(x => ReceivePositionalInputAt(touchInput.GetTouchPosition(x) ?? new Vector2(float.MinValue))).Count();
            else if (IsHovered)
                count = SentakkiActionInputManager.PressedActions.Where(x => x < SentakkiAction.Key1).Count();

            trackedKeys.Value = count;
        }
    }
}
