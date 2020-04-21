// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Sentakki.Judgements;
using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Objects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public abstract class SentakkiHitObject : HitObject
    {
        public override Judgement CreateJudgement() => new SentakkiJudgement();

        public Color4 NoteColor { get; set; }
        public Vector2 endPosition { get; set; }
        public virtual float Angle { get; set; }

        public Vector2 Position { get; set; }

        public float X => Position.X;
        public float Y => Position.Y;

        protected override HitWindows CreateHitWindows() => new SentakkiHitWindows();
    }
}
