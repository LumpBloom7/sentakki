using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring;

public partial class SentakkiHealthProcessor : AccumulatingHealthProcessor
{
    public SentakkiHealthProcessor() : base(0)
    {
    }

    private float healthMultiplier = 0;

    public override void ApplyBeatmap(IBeatmap beatmap)
    {
        base.ApplyBeatmap(beatmap);
        float maxHP = 0;
        foreach (var ho in EnumerateHitObjects(beatmap))
            maxHP += healthForResult(ho.CreateJudgement().MaxResult);

        healthMultiplier = 1 / maxHP;
    }

    protected override double GetHealthIncreaseFor(JudgementResult result) => healthForResult(result.Type) * healthMultiplier;

    private static float healthForResult(HitResult result)
    {
        return result switch
        {
            HitResult.Great => 1,
            HitResult.Good => 2 / 3f,
            HitResult.Ok => 1 / 3f,
            _ => 0,
        };
    }
}
