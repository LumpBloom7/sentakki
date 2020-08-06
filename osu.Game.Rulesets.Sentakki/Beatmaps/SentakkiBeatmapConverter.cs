using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Sentakki.Beatmaps
{
    [Flags]
    public enum ConversionExperiments
    {
        none = 0,
        twins = 1,
        touch = 2,
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

        protected override IEnumerable<SentakkiHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap)
        {
            if ((original as IHasCombo).NewCombo)
                patternGen.CreateNewPattern();

            foreach (var note in patternGen.GenerateNewNote(original).ToList())
                yield return note;
        }

        protected override Beatmap<SentakkiHitObject> CreateBeatmap() => new SentakkiBeatmap();
    }
}
