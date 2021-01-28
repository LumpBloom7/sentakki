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
using osu.Framework.Graphics.Pooling;
using osu.Framework.Allocation;
using System;

namespace osu.Game.Rulesets.Sentakki.UI.Components
{
    public class TouchVisualization : CompositeDrawable
    {
        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

        private DrawablePool<TouchPointer> pointerPool;


        private int leftCount;
        private int rightCount;

        public Dictionary<TouchSource, TouchPointer> InUsePointers = new Dictionary<TouchSource, TouchPointer>();


        public TouchVisualization()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(pointerPool = new DrawablePool<TouchPointer>(2));
        }


        protected override void Update()
        {
            base.Update();

            var touchInput = SentakkiActionInputManager.CurrentState.Touch;
            foreach (var point in touchInput.ActiveSources)
            {
                Vector2 newPos = ToLocalSpace(touchInput.GetTouchPosition(point) ?? Vector2.Zero) - OriginPosition;
                if (!InUsePointers.TryGetValue(point, out TouchPointer pointer))
                {
                    bool useLeftHand = false;

                    if (newPos.X < 0)
                    {
                        if (leftCount <= rightCount)
                            useLeftHand = true;
                    }
                    else
                    {
                        if (leftCount < rightCount)
                            useLeftHand = true;
                    }

                    if (useLeftHand) ++leftCount;
                    else ++rightCount;
                    Console.WriteLine(leftCount + ", " + rightCount);

                    AddInternal(pointer = pointerPool.Get());
                    pointer.Scale = new Vector2(useLeftHand ? -1 : 1, 1);
                    InUsePointers[point] = pointer;
                }
                pointer.Position = newPos;
            }

            foreach (var pair in InUsePointers)
            {
                if (Time.Current - pair.Value.LastActiveTime >= 50 && pair.Value.InUse)
                    pair.Value.FinishUsage();
                else if (!pair.Value.IsInUse)
                {
                    InUsePointers.Remove(pair.Key);/*
                    if (pair.Value.LeftHand) --leftCount;
                    else --rightCount; */
                }
            }
        }

        public class TouchPointer : PoolableDrawable
        {
            public override bool RemoveCompletedTransforms => false;

            public double LastActiveTime;
            public bool LeftHand => Scale.X == -1;

            public bool InUse;

            [BackgroundDependencyLoader]
            private void load()
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
                        Colour = ColourInfo.GradientVertical(Color4.White, Color4.Gray)
                    },
                    new SpriteIcon
                    {
                        Icon = FontAwesome.Regular.HandPaper,
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = Color4.Gray,
                    },
                };
            }

            public new Vector2 Position
            {
                get => base.Position;
                set
                {
                    base.Position = value;
                    LastActiveTime = Time.Current;
                }
            }
            protected override void PrepareForUse()
            {
                this.FadeIn();
                LastActiveTime = Time.Current;
                LifetimeStart = Time.Current;
                InUse = true;
            }

            public void FinishUsage()
            {
                this.FadeOut(50).Expire(false);
                InUse = false;
            }
        }
    }
}
