using osu.Framework.Bindables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Sentakki.Judgements;
using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public abstract class SentakkiHitObject : HitObject, IHasPosition
    {
        public virtual bool IsBreak { get; set; } = false;
        public virtual bool HasTwin { get; set; } = false;

        public override Judgement CreateJudgement() => IsBreak ? new SentakkiBreakJudgement() : new SentakkiJudgement();

        public virtual Color4 NoteColor => IsBreak ? Color4.OrangeRed : (HasTwin ? Color4.Gold : Color4Extensions.FromHex("ff0064"));
        public virtual Vector2 EndPosition { get; set; }
        public virtual float Angle
        {
            get => BindableAngle.Value;
            set => BindableAngle.Value = value;
        }

        public readonly Bindable<float> BindableAngle = new Bindable<float>(0);

        public Vector2 Position { get; set; }

        public float X => Position.X;
        public float Y => Position.Y;

        protected override HitWindows CreateHitWindows() => new SentakkiHitWindows();
    }
}
