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
        public CircularContainer Circle;

        public Func<DrawableMaimaiHitObject, bool> CheckValidation;

        /// <summary>
        /// The action that caused this <see cref="DrawableHit"/> to be hit.
        /// </summary>
        public MaimaiAction? HitAction { get; private set; }

        private bool validActionPressed;

        protected sealed override double InitialLifetimeOffset => 600;
        public override bool HandlePositionalInput => true;

        public DrawableMaimaiHitObject(MaimaiHitObject hitObject)
            : base(hitObject)
        {
            Size = new Vector2(80);
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Alpha = 0.05f;
            AddInternal(Circle = new CircularContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                BorderColour = Color4.White,
                BorderThickness = 10,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    AlwaysPresent = true,
                },
            });
            Position = Vector2.Zero;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            this.FadeIn(400);
            this.MoveTo(HitObject.endPosition, 600);
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
                case ArmedState.Idle:
                    LifetimeStart = HitObject.StartTime - 600;
                    HitAction = null;

                    break;

                case ArmedState.Hit:
                    var b = HitObject.Angle;
                    var a = b * (float)(Math.PI / 180);

                    Circle.ScaleTo(2f, time_fade_hit, Easing.OutCubic)
                       .FadeColour(Color4.Yellow, time_fade_hit, Easing.OutCubic)
                       .MoveToOffset(new Vector2(-(300 * (float)Math.Cos(a)), -(300 * (float)Math.Sin(a))), time_fade_hit, Easing.OutCubic)
                       .FadeOut(time_fade_hit);

                    this.FadeOut(time_fade_hit);

                    break;

                case ArmedState.Miss:
                    var c = HitObject.Angle;
                    var d = c * (float)(Math.PI / 180);

                    Circle.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                       .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                       .MoveToOffset(new Vector2(-(100 * (float)Math.Cos(d)), -(100 * (float)Math.Sin(d))), time_fade_hit, Easing.OutCubic)
                       .FadeOut(time_fade_miss);

                    this.FadeOut(time_fade_miss);

                    break;
            }
        }
    }
}
