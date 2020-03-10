// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using System;
using osuTK;
using osuTK.Graphics;
using System.Linq;

namespace osu.Game.Rulesets.Maimai.Objects.Drawables
{
    public class DrawableMaimaiHitObject : DrawableHitObject<MaimaiHitObject>
    {
        public CircularContainer Circle;
        public HitReceptor Receptor;

        public Func<DrawableMaimaiHitObject, bool> CheckValidation;

        /// <summary>
        /// The action that caused this <see cref="DrawableHit"/> to be hit.
        /// </summary>
        public MaimaiAction? HitAction { get; private set; }

        private bool validActionPressed;

        protected sealed override double InitialLifetimeOffset => 600;
        //public override bool HandlePositionalInput => true;

        public DrawableMaimaiHitObject(MaimaiHitObject hitObject)
            : base(hitObject)
        {
            Size = new Vector2(.8f);
            RelativeSizeAxes = Axes.Both;
            FillMode = FillMode.Fit;
            FillAspectRatio = 1;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            AddRangeInternal(new Drawable[]{
                Receptor = new HitReceptor(),
                Circle = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(80),
                    Masking = true,
                    BorderColour = Color4.White,
                    BorderThickness = 10,
                    Alpha = 0.05f,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true,
                    },
                    Position = Vector2.Zero,
                }
            });

            hitObject.Angle = Utils.GetNotePathFromDegrees(hitObject.endPosition.GetDegreesFromPosition(Circle.AnchorPosition) * 4);
            Rotation = hitObject.Angle;
            Position = Vector2.Zero;

        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            var b = HitObject.endPosition.GetDegreesFromPosition(Circle.AnchorPosition) * 4;
            var a = b *= (float)(Math.PI / 180);

            Circle.FadeIn(400);
            Circle.MoveToY(this.RelativeToAbsoluteFactor.Y / 2, 600);
            Receptor.MoveToY(this.RelativeToAbsoluteFactor.Y / 2, 600);
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
                ApplyResult(r => r.Type = this.Receptor.IsHovered ? HitResult.Perfect : HitResult.Miss);
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);

            switch (state)
            {
                case ArmedState.Hit:
                    this.ScaleTo(5, 1500, Easing.OutQuint).FadeOut(1500, Easing.OutQuint).Expire();
                    break;

                case ArmedState.Miss:
                    const double duration = 1000;

                    this.ScaleTo(1.2f, duration, Easing.OutQuint);
                    //this.MoveToOffset(new Vector2(0, 10), duration, Easing.In);
                    this.FadeColour(Color4.Red.Opacity(0.5f), duration / 2, Easing.OutQuint).Then().FadeOut(duration / 2, Easing.InQuint).Expire();
                    break;
            }
        }

        public class HitReceptor : CompositeDrawable
        {
            // IsHovered is used
            public override bool HandlePositionalInput => true;

            public HitReceptor()
            {
                Size = new Vector2(80);

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                CornerRadius = 40;
                CornerExponent = 2;
            }
        }
    }
}
