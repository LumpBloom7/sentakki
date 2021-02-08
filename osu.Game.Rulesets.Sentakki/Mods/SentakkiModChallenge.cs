using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Rulesets.UI;
using System.ComponentModel;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModChallenge : Mod, IApplicableToDrawableRuleset<SentakkiHitObject>, IApplicableToHealthProcessor
    {
        public override string Name => "Challenge";
        public override string Description => "You only get a small margin for errors.";
        public override string Acronym => "C";

        public override IconUsage? Icon => FontAwesome.Solid.HeartBroken;
        public override ModType Type => ModType.DifficultyIncrease;

        public override bool Ranked => false;

        public override double ScoreMultiplier => 1.00;

        public bool RestartOnFail => false;
        public bool PerformFail() => true;

        public enum Lives
        {
            [Description("1")] One,
            [Description("5")] Five,
            [Description("10")] Ten,
            [Description("20")] Twenty,
            [Description("50")] Fifty,
            [Description("100")] Hundred,
            [Description("200")] TwoHundred,
            [Description("300")] ThreeHundred,
        }

        [SettingSource("Number of Lives", "The number of lives you start with.")]
        public Bindable<Lives> LiveSetting { get; } = new Bindable<Lives>
        {
            Default = Lives.Ten,
            Value = Lives.Ten,
        };

        public BindableInt LivesLeft = new BindableInt();

        public void ApplyToDrawableRuleset(DrawableRuleset<SentakkiHitObject> drawableRuleset)
        {
            LivesLeft.Value = int.Parse(LiveSetting.Value.GetDescription());
            (drawableRuleset.Playfield as SentakkiPlayfield).AddDrawable(new LiveCounter().With(d => d.LivesLeft.BindTo(LivesLeft)));
        }

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            healthProcessor.FailConditions += FailCondition;
        }

        protected bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
        {
            if (!(result.Judgement is SentakkiJudgement) || result.HitObject is ScorePaddingObject)
                return false;

            int newValue = LivesLeft.Value;

            switch (result.Type)
            {
                case HitResult.Great:
                    newValue -= 1;
                    break;
                case HitResult.Good:
                    newValue -= 2;
                    break;
                case HitResult.Miss:
                    newValue -= 5;
                    break;
            }
            if (newValue < 0) newValue = 0;
            LivesLeft.Value = newValue;

            return LivesLeft.Value <= 0;
        }
    }
}
