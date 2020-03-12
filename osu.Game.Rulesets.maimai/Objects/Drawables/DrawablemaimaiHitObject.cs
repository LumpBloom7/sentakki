// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Maimai.UI;
using osu.Game.Rulesets.Scoring;
using System;
using osuTK;
using osuTK.Graphics;
using System.Linq;

namespace osu.Game.Rulesets.Maimai.Objects.Drawables
{
    public class DrawableMaimaiHitObject : DrawableHitObject<MaimaiHitObject>
    {

        public Func<DrawableMaimaiHitObject, bool> CheckValidation;
        public HitReceptor MaimaiNote;
        public CircularProgress hitObjectLine;

        /// <summary>
        /// The action that caused this <see cref="DrawableHit"/> to be hit.
        /// </summary>
        public MaimaiAction? HitAction { get; private set; }

        private bool validActionPressed;

        protected sealed override double InitialLifetimeOffset => 800;
        public override bool HandlePositionalInput => true;

        public DrawableMaimaiHitObject(MaimaiHitObject hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.Both;
            CornerRadius = 120;
            CornerExponent = 2;
            Size = Vector2.Zero;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            //Position = hitObject.Position;
            AddRangeInternal(new Drawable[] {
                hitObjectLine = new CircularProgress
                {
                    Size = new Vector2(MaimaiPlayfield.noteStartDistance*2),
                    RelativePositionAxes = Axes.None,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = HitObject.NoteColor,
                    InnerRadius = .025f,
                    RelativeSizeAxes = Axes.None,
                    Rotation =  -45 +HitObject.Angle,
                    Current = new Bindable<double>(0.25),
                    Alpha = 0f

                },
                MaimaiNote = new HitReceptor(HitObject)


            });

        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            MaimaiNote.FadeInFromZero(500).Append(b => b.ScaleTo(1f, 500)).Then(b => b.MoveTo(HitObject.endPosition, 300));
            //hitObjectLine.ScaleTo(1f, 500).Then(l => l.ScaleTo(4.54545455f, 300));
            hitObjectLine.FadeTo(.75f, 500).Then(h => h.ResizeTo(600, 300));
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (timeOffset >= 0)
                ApplyResult(r => r.Type = MaimaiNote.IsHovered ? HitResult.Perfect : HitResult.Miss);
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);

            const double time_fade_hit = 250, time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Hit:
                    var b = HitObject.Angle + 90;
                    var a = b * (float)(Math.PI / 180);



                    MaimaiNote.ScaleTo(2f, time_fade_hit, Easing.OutCubic)
                       .FadeColour(Color4.Yellow, time_fade_hit, Easing.OutCubic)
                       .MoveToOffset(new Vector2(-(500 * (float)Math.Cos(a)), -(500 * (float)Math.Sin(a))), time_fade_hit, Easing.OutCubic)
                       .FadeOut(time_fade_hit);

                    MaimaiNote.FadeOut(time_fade_hit);
                    hitObjectLine.FadeOut();
                    this.ScaleTo(1f, time_fade_hit).Expire();

                    break;

                case ArmedState.Miss:
                    var c = HitObject.Angle + 90;
                    var d = c * (float)(Math.PI / 180);

                    MaimaiNote.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                       .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                       .MoveToOffset(new Vector2(-(100 * (float)Math.Cos(d)), -(100 * (float)Math.Sin(d))), time_fade_hit, Easing.OutCubic)
                       .FadeOut(time_fade_miss);

                    MaimaiNote.FadeOut(time_fade_miss);
                    hitObjectLine.FadeOut();
                    this.ScaleTo(1f, time_fade_miss).Expire();

                    break;
            }
        }

        public class HitReceptor : CompositeDrawable
        {
            public override bool HandlePositionalInput => true;

            public MaimaiHitObject HitObject;
            public HitReceptor(MaimaiHitObject HitObject)
            {
                this.HitObject = HitObject;
                RelativeSizeAxes = Axes.None;
                CornerRadius = 120;
                CornerExponent = 2;
                Scale = new Vector2(.2f);
                Size = new Vector2(240);
                Origin = Anchor.Centre;
                Anchor = Anchor.Centre;
                Alpha = .05f;
                Position = HitObject.Position;
            }

            [BackgroundDependencyLoader(true)]
            private void load(TextureStore textures)
            {
                AddRangeInternal(new Drawable[] {
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(80),
                    Colour = HitObject.NoteColor,
                    Child = new Sprite
                    {

                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(1.28125f),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Texture = textures.Get("Gameplay/osu/ring-glow"),
                        Blending = BlendingParameters.Additive,
                        Alpha = 0.5f
                    }

                },
                new CircularContainer
                {
                    Size = new Vector2(80),
                    Masking = true,
                    BorderColour = HitObject.NoteColor,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    BorderThickness = 15,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true,
                    },
                },
                new CircularContainer
                {
                    Size = new Vector2(80),
                    Masking = true,
                    BorderColour = Color4.Black,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    BorderThickness = 3,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true,
                    },
                },
            });
            }

        }
    }
}
