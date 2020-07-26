using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;
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
        patternv2 = 4,
        slide = 8
    }

    public class SentakkiBeatmapConverter : BeatmapConverter<SentakkiHitObject>
    {
        // todo: Check for conversion types that should be supported (ie. Beatmap.HitObjects.Any(h => h is IHasXPosition))
        // https://github.com/ppy/osu/tree/master/osu.Game/Rulesets/Objects/Types
        public override bool CanConvert() => Beatmap.HitObjects.All(h => h is IHasPosition);

        public Bindable<ConversionExperiments> EnabledExperiments = new Bindable<ConversionExperiments>();

        private SentakkiPatternGenerator patternGen;

        private readonly Random random;
        private readonly Random random2;

        public SentakkiBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
            patternGen = new SentakkiPatternGenerator(beatmap);
            var difficulty = beatmap.BeatmapInfo.BaseDifficulty;
            int seed = ((int)MathF.Round(difficulty.DrainRate + difficulty.CircleSize) * 20) + (int)(difficulty.OverallDifficulty * 41.2) + (int)MathF.Round(difficulty.ApproachRate);
            random = new Random(seed);
            random2 = new Random(seed);
            patternGen.Experiments.BindTo(EnabledExperiments);
        }

        protected override IEnumerable<SentakkiHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap)
        {
            Vector2 CENTRE_POINT = new Vector2(256, 192);
            Vector2 newPos = (original as IHasPosition)?.Position ?? Vector2.Zero;
            newPos.Y = 384 - newPos.Y;

            int lane = CENTRE_POINT.GetDegreesFromPosition(newPos).GetNoteLaneFromDegrees();
            List<SentakkiHitObject> objects = new List<SentakkiHitObject>();

            if (EnabledExperiments.Value.HasFlag(ConversionExperiments.patternv2))
            {
                if ((original as IHasCombo).NewCombo)
                    patternGen.CreateNewPattern();

                foreach (var note in patternGen.GenerateNewNote(original).ToList())
                    yield return note;
            }
            else
            {
                switch (original)
                {
                    case IHasPathWithRepeats _:
                        objects.AddRange(Conversions.CreateHoldNote(original, lane, beatmap, random, EnabledExperiments.Value));
                        break;

                    case IHasDuration _:
                        objects.Add(Conversions.CreateTouchHold(original));
                        break;

                    default:
                        if (EnabledExperiments.Value.HasFlag(ConversionExperiments.touch) && (random2.Next() % 10 == 0))
                            objects.AddRange(Conversions.CreateTouchNote(original, lane, random, EnabledExperiments.Value));
                        else
                            objects.AddRange(Conversions.CreateTapNote(original, lane, random, EnabledExperiments.Value));
                        break;
                }

                foreach (var hitObject in objects)
                    yield return hitObject;

                yield break;
            }
        }

        protected override Beatmap<SentakkiHitObject> CreateBeatmap() => new SentakkiBeatmap();
    }
}
