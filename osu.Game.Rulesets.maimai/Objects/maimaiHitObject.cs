// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Maimai.Objects
{
    public class MaimaiHitObject : HitObject
    {
        public Color4 NoteColor { get; set; }
        public Vector2 endPosition { get; set; }
        public float Angle { get; set; }
        public int path { get; set; }
        public override Judgement CreateJudgement() => new Judgement();

        public Vector2 Position { get; set; }

        public float X => Position.X;
        public float Y => Position.Y;
    }
}
