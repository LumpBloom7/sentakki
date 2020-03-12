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
            CornerRadius = 120;
            CornerExponent = 2;
            Scale = new Vector2(.2f);
            Size = new Vector2(240);
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Alpha = 0.05f;
            AddRangeInternal(new Drawable[] {
                new CircularContainer
                {
                    Size = new Vector2(80),
                    Masking = true,
                    BorderColour = Color4.Black,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    BorderThickness = 18,
                    Children = new Drawable[]{
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true,
                        },
                        new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
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
                            RelativeSizeAxes = Axes.Both,
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
                        new Circle
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(.2f),
                            Colour = HitObject.NoteColor,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                        },
                        new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(.2f),
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
                    }
                },
            });

            Position = hitObject.Position;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            AddInternal(new Container
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

            });
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            this.FadeInFromZero(500).Append(b => b.ScaleTo(1f, 500)).Then(b => b.MoveTo(HitObject.endPosition, 300));
        }

        protected override IEnumerable<HitSampleInfo> GetSamples() => new[]
        {
            new HitSampleInfo
            {
                Bank = SampleControlPoint.DEFAULT_BANK,
                Name = HitSampleInfo.HIT_NORMAL,
            }
        };

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (timeOffset >= 0)
                ApplyResult(r => r.Type = this.IsHovered ? HitResult.Perfect : HitResult.Miss);
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

                    this.ScaleTo(2f, time_fade_hit, Easing.OutCubic)
                       .FadeColour(Color4.Yellow, time_fade_hit, Easing.OutCubic)
                       .MoveToOffset(new Vector2(-(500 * (float)Math.Cos(a)), -(500 * (float)Math.Sin(a))), time_fade_hit, Easing.OutCubic)
                       .FadeOut(time_fade_hit);

                    this.FadeOut(time_fade_hit);

                    break;

                case ArmedState.Miss:
                    var c = HitObject.Angle + 90;
                    var d = c * (float)(Math.PI / 180);

                    this.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                       .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                       .MoveToOffset(new Vector2(-(100 * (float)Math.Cos(d)), -(100 * (float)Math.Sin(d))), time_fade_hit, Easing.OutCubic)
                       .FadeOut(time_fade_miss);

                    this.FadeOut(time_fade_miss);

                    break;
            }
        }
    }
}
