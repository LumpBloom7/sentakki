using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sentakki.Configuration;
using osuTK;
using osuTK.Graphics;
using System;
using osu.Game.Online.API;
using osu.Game.Users;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.UI.Components
{
    public class TouchVisualization : CompositeDrawable
    {
        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

        private readonly Container dots;

        public TouchVisualization()
        {
            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;
            InternalChildren = new Drawable[]
            {
                dots = new Container()
            };
        }

        protected override void Update()
        {
            base.Update();
            dots.Clear();

            var touchInput = SentakkiActionInputManager.CurrentState.Touch;
            foreach (var points in touchInput.ActiveSources)
            {
                dots.Add(new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(20),
                    Position = ToLocalSpace(touchInput.GetTouchPosition(points) ?? new Vector2(float.MinValue)),
                    Colour = Color4.Green,
                });
            }
        }
    }
}
