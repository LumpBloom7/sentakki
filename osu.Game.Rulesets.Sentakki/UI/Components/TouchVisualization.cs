using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Input;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Colour;
using System.Collections.Generic;

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
            for (int i = 0; i < 10; ++i)
            {
                dots.Add(new TouchPointer((TouchSource)i));
            }
        }

        protected override void Update()
        {
            base.Update();

            var touchInput = SentakkiActionInputManager.CurrentState.Touch;
            List<bool> toShow = new List<bool>(new bool[10]);
            foreach (var point in touchInput.ActiveSources)
            {
                toShow[(int)point] = true;
                dots.Children[(int)point].Position = ToLocalSpace(touchInput.GetTouchPosition(point) ?? Vector2.Zero);
            }

            for (int i = 0; i < 10; ++i)
            {
                if (toShow[i])
                    dots.Children[i].Show();
                else
                    dots.Children[i].Hide();
            }
        }

        public class TouchPointer : VisibilityContainer
        {
            protected override bool StartHidden => true;
            public TouchPointer(TouchSource source)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Size = new Vector2(50);
                InternalChildren = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.HandPaper,
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = ColourInfo.GradientVertical(Color4.White, colourForSource(source, true))
                    },
                    new SpriteIcon
                    {
                        Icon = FontAwesome.Regular.HandPaper,
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = colourForSource(source),
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = ((int)source+1).ToString(),
                        Font = OsuFont.Torus.With(size: 18,weight: FontWeight.Medium),
                        Colour = colourForSource(source),
                        Position = new Vector2(.07f,.175f),
                        RelativePositionAxes = Axes.Both,
                    }
                };
            }
            protected override void PopIn()
            {
                FinishTransforms();
                this.ScaleTo(1, 50, Easing.OutBack).FadeIn(50);
            }

            protected override void PopOut()
            {
                FinishTransforms();
                this.FadeOut(200).Then().ScaleTo(0);
            }

            private Color4 colourForSource(TouchSource source, bool light = false)
            {
                OsuColour colours = new OsuColour();
                switch (source)
                {
                    case TouchSource.Touch1:
                        return light ? colours.RedLight : colours.Red;
                    case TouchSource.Touch2:
                        return light ? colours.PurpleLight : colours.Purple;
                    case TouchSource.Touch3:
                        return light ? colours.PinkLight : colours.Pink;
                    case TouchSource.Touch4:
                        return light ? colours.BlueLight : colours.BlueDarker;
                    case TouchSource.Touch5:
                        return light ? colours.YellowLight : colours.Yellow;
                    case TouchSource.Touch6:
                        return light ? colours.GreenLight : colours.Green;
                    case TouchSource.Touch7:
                        return light ? colours.Sky : colours.GreySky;
                    case TouchSource.Touch8:
                        return light ? colours.Cyan : colours.GreyCyan;
                    case TouchSource.Touch9:
                        return light ? colours.Lime : colours.GreyLime;
                    case TouchSource.Touch10:
                        return light ? colours.GreyViolet : colours.Violet;
                }
                return Color4.Blue;
            }
        }
    }
}
