using System;
using System.ComponentModel;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
using osu.Game.Rulesets.Sentakki.Localisation.Mods;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sentakki.Mods;

public class SentakkiModChallenge : ModFailCondition, IApplicableToDrawableRuleset<SentakkiHitObject>
{
    public override string Name => "Challenge";
    public override LocalisableString Description => SentakkiModChallengeStrings.ModDescription;
    public override string Acronym => "C";

    public override string ExtendedIconInformation => $"{(int)LiveSetting.Value}♥";

    public override ModType Type => ModType.DifficultyIncrease;

    public override bool RequiresConfiguration => true;

    public override double ScoreMultiplier => 1.00;
    public override bool Ranked => true;

    public override Type[] IncompatibleMods =>
    [
        typeof(ModRelax),
        typeof(ModFailCondition),
        typeof(ModAutoplay),
        typeof(ModNoFail)
    ];

    public enum Lives
    {
        [Description("5")]
        Five = 5,

        [Description("10")]
        Ten = 10,

        [Description("20")]
        Twenty = 20,

        [Description("50")]
        Fifty = 50,

        [Description("100")]
        Hundred = 100,

        [Description("200")]
        TwoHundred = 200,

        [Description("300")]
        ThreeHundred = 300,
    }

    [SettingSource(typeof(SentakkiModChallengeStrings), nameof(SentakkiModChallengeStrings.NumberOfLives), nameof(SentakkiModChallengeStrings.NumberOfLivesDescription))]
    public Bindable<Lives> LiveSetting { get; } = new Bindable<Lives>
    {
        Default = Lives.Fifty,
        Value = Lives.Fifty,
    };

    [JsonIgnore]
    private BindableInt livesLeft = null!;

    public void ApplyToDrawableRuleset(DrawableRuleset<SentakkiHitObject> drawableRuleset)
    {
        int maxLives = (int)LiveSetting.Value;
        livesLeft = new BindableInt
        {
            Value = maxLives,
            MaxValue = maxLives,
        };

        ((SentakkiPlayfield)drawableRuleset.Playfield).AccentContainer.Add(new LiveCounter(livesLeft));
    }

    protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
    {
        if (result.Judgement is not SentakkiJudgement || result.HitObject is ScorePaddingObject)
            return false;

        int newValue = livesLeft.Value;

        switch (result.Type)
        {
            case HitResult.Good:
                newValue -= 1;
                break;

            case HitResult.Ok:
                newValue -= 2;
                break;

            case HitResult.Miss:
                newValue -= 5;
                break;
        }

        if (newValue < 0) newValue = 0;
        livesLeft.Value = newValue;

        return livesLeft.Value <= 0;
    }
}
