using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI;
using static osu.Game.Input.Handlers.ReplayInputHandler;
using osuTK;
using System.Linq;

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

            private BindableInt currentKeys = new BindableInt(0);
            public LaneReceptor()
            {
                Position = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, 0);
                Size = new Vector2(300);

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                CornerRadius = 150;
                CornerExponent = 2;
                currentKeys.BindValueChanged(handleKeyPress);
            }

            protected override void Update()
            {
                base.Update();
                if (IsHovered)
                    currentKeys.Value = SentakkiActionInputManager.PressedActions.Where(x => x < SentakkiAction.Key1).Count();
                else
                    currentKeys.Value = 0;
            }

            private void handleKeyPress(ValueChangedEvent<int> keys)
            {
                if (keys.NewValue > keys.OldValue || keys.NewValue == 0)
                    SentakkiActionInputManager.TriggerReleased(SentakkiAction.Key1 + ((Lane)Parent).LaneNumber);

                if (keys.NewValue > keys.OldValue)
                    SentakkiActionInputManager.TriggerPressed(SentakkiAction.Key1 + ((Lane)Parent).LaneNumber);
            }
        }
    }
}
