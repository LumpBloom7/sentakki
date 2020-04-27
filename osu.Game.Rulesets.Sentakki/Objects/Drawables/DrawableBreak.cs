// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using System.Diagnostics;
using osu.Game.Skinning;
using osu.Game.Audio;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableBreak : DrawableTap
    {
        private readonly Container<DrawableChild> children;

        private readonly SkinnableSound breakSound;
        public DrawableBreak(SentakkiHitObject hitObject) : base(hitObject)
        {
            AddRangeInternal(new Drawable[]{
                children = new Container<DrawableChild>(),
                breakSound = new SkinnableSound(new SampleInfo("Break"))
            });
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                {
                    ApplyResult(r => r.Type = HitResult.Miss);
                }
                return;
            }

            if (HitObject.HitWindows.ResultFor(timeOffset) == HitResult.Miss && Time.Current < HitObject.StartTime) return;

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
            {
                return;
            }
            ApplyResult(r => r.Type = result);
            if (result != HitResult.Miss)
                breakSound?.Play();
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);

            switch (hitObject)
            {
                case DrawableChild child:
                    children.Add(child);
                    break;
            }
        }
        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Break.Child child:
                    return new DrawableChild(child);
            }

            return base.CreateNestedHitObject(hitObject);
        }

        public class DrawableChild : DrawableSentakkiHitObject
        {
            public DrawableChild(Break.Child content) : base(content) { }
            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                if ((Parent.Parent as DrawableHitObject).Result.HasResult)
                    ApplyResult(r => r.Type = (Parent.Parent as DrawableHitObject).Result.Type);
            }
        }
    }
}