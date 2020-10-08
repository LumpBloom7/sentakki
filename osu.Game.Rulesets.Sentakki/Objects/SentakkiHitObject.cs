using osu.Framework.Bindables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Sentakki.Judgements;
using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Objects;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using System.Threading;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public abstract class SentakkiHitObject : HitObject
    {
        public bool HasTwin { get; set; }

        public override Judgement CreateJudgement() => new SentakkiJudgement();

        public Bindable<Color4> ColourBindable = new Bindable<Color4>();
        public Color4 NoteColour
        {
            get => ColourBindable.Value;
            private set => ColourBindable.Value = value;
        }

        protected virtual Color4 DefaultNoteColour => Color4Extensions.FromHex("FF0064");

        protected override HitWindows CreateHitWindows() => new SentakkiHitWindows();

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            bool isBreak = this is SentakkiLanedHitObject x && x.Break;

            NoteColour = isBreak ? Color4.OrangeRed : (HasTwin ? Color4.Gold : DefaultNoteColour);
        }
    }
}
