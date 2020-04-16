// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Maimai.Judgements;
using osu.Game.Rulesets.Maimai.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Objects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Maimai.Objects
{
    public abstract class MaimaiHitObject : HitObject
    {
        public override Judgement CreateJudgement() => new MaimaiJudgement();

        public Color4 NoteColor { get; set; }
        public Vector2 endPosition { get; set; }
        public virtual float Angle { get; set; }

        public Vector2 Position { get; set; }

        public float X => Position.X;
        public float Y => Position.Y;

        protected override HitWindows CreateHitWindows() => new MaimaiHitWindows();
    }
}
