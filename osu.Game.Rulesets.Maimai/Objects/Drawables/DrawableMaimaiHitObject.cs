// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;
using System;

namespace osu.Game.Rulesets.Maimai.Objects.Drawables
{
    public class DrawableMaimaiHitObject : DrawableHitObject<MaimaiHitObject>
    {
        public bool IsHidden = false;
        public Func<DrawableMaimaiHitObject, bool> CheckValidation;

        public MaimaiAction[] HitActions { get; set; } = new[]
        {
            MaimaiAction.Button1,
            MaimaiAction.Button2,
        };

        public DrawableMaimaiHitObject(MaimaiHitObject hitObject)
            : base(hitObject)
        {
        }
    }
}
