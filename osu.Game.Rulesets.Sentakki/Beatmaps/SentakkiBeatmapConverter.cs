using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Beatmaps
{
    [Flags]
    public enum ConversionExperiments
    {
        none = 0,
        twinNotes = 1,
        twinSlides = 2,
    }

    public class SentakkiBeatmapConverter : BeatmapConverter<SentakkiHitObject>
    {
        // todo: Check for conversion types that should be supported (ie. Beatmap.HitObjects.Any(h => h is IHasXPosition))
        // https://github.com/ppy/osu/tree/master/osu.Game/Rulesets/Objects/Types
        public override bool CanConvert() => Beatmap.HitObjects.All(h => h is IHasPosition);

        public Bindable<ConversionExperiments> EnabledExperiments = new Bindable<ConversionExperiments>();

        private SentakkiPatternGenerator patternGen;

        public SentakkiBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
            patternGen = new SentakkiPatternGenerator(beatmap);
            patternGen.Experiments.BindTo(EnabledExperiments);
        }

        protected override Beatmap<SentakkiHitObject> ConvertBeatmap(IBeatmap original, CancellationToken cancellationToken)
        {
            var convertedBeatmap = base.ConvertBeatmap(original, cancellationToken);

            // We don't use any of the standard difficulty values
            // But we initialize to defaults so HR can adjust HitWindows in a controlled manner
            // We clone beforehand to avoid altering the original (it really should be readonly :P)
            convertedBeatmap.BeatmapInfo = convertedBeatmap.BeatmapInfo.Clone();
            convertedBeatmap.BeatmapInfo.BaseDifficulty = new BeatmapDifficulty();

            return convertedBeatmap;
        }

        protected override IEnumerable<SentakkiHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            if ((original as IHasCombo).NewCombo)
                patternGen.CreateNewPattern();

            foreach (var note in patternGen.GenerateNewNote(original).ToList())
                yield return note;
        }

        protected override Beatmap<SentakkiHitObject> CreateBeatmap() => new SentakkiBeatmap();
    }
}
