using System.Linq;
using System.Threading;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Beatmaps.Converter;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Beatmaps;

public class CompositeBeatmapConverter : BeatmapConverter<SentakkiHitObject>
{
    public override bool CanConvert() => Beatmap.HitObjects.All(h => h is IHasPosition) || Beatmap.BeatmapInfo.Ruleset.ShortName == "Sentakki";

    public ConversionFlags flags;

    private Ruleset ruleset;

    public CompositeBeatmapConverter(IBeatmap beatmap, Ruleset ruleset) : base(beatmap, ruleset)
    {
        this.ruleset = ruleset;
    }

    protected override Beatmap<SentakkiHitObject> ConvertBeatmap(IBeatmap original, CancellationToken cancellationToken)
    {
        if (Beatmap.HitObjects.All(h => h is SentakkiHitObject))
        {
            return base.ConvertBeatmap(original, cancellationToken);
        }

        BeatmapConverter<SentakkiHitObject> converter;

        if (flags.HasFlag(ConversionFlags.oldConverter))
            converter = new SentakkiBeatmapConverterOld(original, ruleset) { ConversionFlags = flags };
        else
            converter = new SentakkiBeatmapConverter(original, ruleset) { ConversionFlags = flags };

        return ((SentakkiBeatmap)converter.Convert(cancellationToken)) ?? base.ConvertBeatmap(original, cancellationToken);
    }

    protected override Beatmap<SentakkiHitObject> CreateBeatmap() => new SentakkiBeatmap();
}
