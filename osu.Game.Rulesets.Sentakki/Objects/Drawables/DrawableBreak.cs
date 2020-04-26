// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableBreak : DrawableTap
    {
        private readonly Container<DrawableChild> children;
        public DrawableBreak(SentakkiHitObject hitObject) : base(hitObject)
        {
            AddInternal(
                children = new Container<DrawableChild>()
            );
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