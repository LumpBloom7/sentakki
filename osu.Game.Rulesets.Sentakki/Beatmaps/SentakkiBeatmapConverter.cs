using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Beatmaps
{
    [Flags]
    public enum ConversionExperiments
    {
        none = 0,
        twinNotes = 1,
        twinSlides = 2,
        fanSlides = 4
    }

    public class SentakkiBeatmapConverter : BeatmapConverter<SentakkiHitObject>
    {
        // Conversion flags
        public ConversionExperiments EnabledExperiments;
        public bool ClassicMode;

        public static readonly List<Vector2> VALID_TOUCH_POSITIONS;
        static SentakkiBeatmapConverter()
        {
            var tmp = new List<Vector2>(){
                Vector2.Zero
            };
            foreach (float angle in SentakkiPlayfield.LANEANGLES)
            {
                tmp.Add(SentakkiExtensions.GetCircularPosition(190, angle - 22.5f));
                tmp.Add(SentakkiExtensions.GetCircularPosition(130, angle));
            }
            VALID_TOUCH_POSITIONS = tmp;
        }

        // todo: Check for conversion types that should be supported (ie. Beatmap.HitObjects.Any(h => h is IHasXPosition))
        // https://github.com/ppy/osu/tree/master/osu.Game/Rulesets/Objects/Types
        public override bool CanConvert() => Beatmap.HitObjects.All(h => h is IHasPosition);

        private readonly SentakkiPatternGenerator patternGenerator;

        private readonly Dictionary<Vector2, double> endTimes = new Dictionary<Vector2, double>();

        public SentakkiBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
            patternGenerator = new SentakkiPatternGenerator(beatmap);
        }

        protected override Beatmap<SentakkiHitObject> ConvertBeatmap(IBeatmap original, CancellationToken cancellationToken)
        {
            var convertedBeatmap = base.ConvertBeatmap(original, cancellationToken);

            // We don't use any of the standard difficulty values
            // But we initialize to defaults so HR can adjust HitWindows in a controlled manner
            // We clone beforehand to avoid altering the original (it really should be readonly :P)
            convertedBeatmap.BeatmapInfo = convertedBeatmap.BeatmapInfo.Clone();
            convertedBeatmap.BeatmapInfo.Difficulty = new BeatmapDifficulty();

            return convertedBeatmap;
        }

        protected override Beatmap<SentakkiHitObject> CreateBeatmap() => new SentakkiBeatmap();

        protected override IEnumerable<SentakkiHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            if ((original as IHasCombo)?.NewCombo ?? false)
                patternGenerator.StartNextPattern();

            switch (original)
            {
                case IHasPathWithRepeats _:
                    return convertSlider(original);
                case IHasDuration _:
                    return convertSpinner(original);
                default:
                    return convertHitCircle(original);
            }
        }

        #region std -> sentakki conversion rules
        private IEnumerable<SentakkiHitObject> convertHitCircle(HitObject original)
        {
            bool breakNote = original.Samples.Any(s => s.Name == HitSampleInfo.HIT_FINISH);
            bool special = original.Samples.Any(s => s.Name == HitSampleInfo.HIT_WHISTLE);
            bool twin = original.Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP);

            if (!breakNote && !ClassicMode && special)
                yield return createTouchNote(original);
            else
            {
                if (twin && EnabledExperiments.HasFlag(ConversionExperiments.twinNotes))
                    yield return createTapNote(original, true, breakNote);

                yield return createTapNote(original, false, breakNote);
            }
        }

        private IEnumerable<SentakkiHitObject> convertSpinner(HitObject original)
        {
            IHasDuration spinner = (IHasDuration)original;
            if (spinner.Duration >= 100)
            {
                if (!ClassicMode)
                    yield return createTouchHold(original);
                else
                    foreach (var ho in convertSlider(original))
                        yield return ho;
            }
            else
            {
                foreach (var ho in convertHitCircle(original))
                    yield return ho;
            }
        }

        private IEnumerable<SentakkiHitObject> convertSlider(HitObject original)
        {
            IHasDuration slider = (IHasDuration)original;
            bool breakHead = false;
            bool breakTail = false;
            bool special = false;
            bool twin = false;

            IList<IList<HitSampleInfo>> nodeSamples;

            if (original is IHasPathWithRepeats sl)
                nodeSamples = sl.NodeSamples;
            else
                nodeSamples = new List<IList<HitSampleInfo>>
                {
                    original.Samples,
                    original.Samples
                };

            // Check properties for tap
            foreach (var sample in nodeSamples[0])
            {
                if (sample.Name == HitSampleInfo.HIT_FINISH)
                    breakHead = true;

                if (sample.Name == HitSampleInfo.HIT_WHISTLE)
                    special = true;

                if (sample.Name == HitSampleInfo.HIT_CLAP)
                    twin = true;
            }

            foreach (var sample in nodeSamples[^1])
            {
                if (sample.Name == HitSampleInfo.HIT_FINISH)
                {
                    breakTail = true;
                    break;
                }
            }

            // See if we can convert to a slide object
            if (special && slider.Duration >= 350)
            {
                var result = tryConvertSliderToSlide(original, nodeSamples, twin, breakHead, breakTail).ToList();
                if (result.Any())
                {
                    foreach (var ho in result)
                        yield return ho;

                    yield break;
                }
            }

            // Fallback to hold notes
            if (EnabledExperiments.HasFlag(ConversionExperiments.twinNotes))
                if (twin)
                    yield return createHoldNote(original, nodeSamples, true, breakHead);
                else
                    foreach (var note in createTapsFromNodes(original, nodeSamples))
                        yield return note;

            yield return createHoldNote(original, nodeSamples, false, breakHead);
        }

        private IEnumerable<SentakkiHitObject> tryConvertSliderToSlide(HitObject original, IList<IList<HitSampleInfo>> nodeSamples, bool twin = false, bool hasBreakHead = false, bool hasBreakTail = false)
        {
            List<Slide> slides = new List<Slide>();
            List<Tap> taps = new List<Tap>();
            if (EnabledExperiments.HasFlag(ConversionExperiments.twinSlides))
            {
                if (twin)
                    slides.Add((Slide)createSlideNote(original, nodeSamples, true, hasBreakHead, hasBreakTail));
                else
                    taps.AddRange(createTapsFromNodes(original, nodeSamples));
            }

            slides.Add((Slide)createSlideNote(original, nodeSamples, false, hasBreakHead, hasBreakTail));

            slides.RemoveAll(x => x == null);

            // If there is a SlideFan, we always prioritize that, and ignore the rest
            foreach (var slide in slides)
                if (slide.SlideInfoList[0].SlidePathParts[0].Shape == SlidePaths.PathShapes.Fan)
                {
                    yield return slide;
                    yield break;
                }

            // If both slides have the same start lane, we attempt to merge them
            if (slides.Count == 2 && slides[0].Lane == slides[1].Lane)
            {
                // We merge both slides only if they both have the same pattern AND orientation
                if (slides[0].SlideInfoList[0].Equals(slides[1].SlideInfoList[0]))
                    slides[0].SlideInfoList.AddRange(slides[1].SlideInfoList);

                slides.RemoveAt(1);
            }

            if (slides.Count > 0)
            {
                foreach (var slide in slides)
                    yield return slide;

                foreach (var tap in taps)
                    yield return tap;
            }
        }

        #endregion

        #region SentakkiHitObject creation methods

        private SentakkiHitObject createTapNote(HitObject original, bool twin = false, bool isBreak = false)
            => new Tap
            {
                Break = isBreak,
                Lane = patternGenerator.GetNextLane(twin),
                Samples = original.Samples,
                StartTime = original.StartTime,
            };

        private SentakkiHitObject createHoldNote(HitObject original, IList<IList<HitSampleInfo>> samples, bool twin = false, bool isBreak = false)
            => new Hold
            {
                Break = !ClassicMode && isBreak,
                Lane = patternGenerator.GetNextLane(twin),
                NodeSamples = samples,
                Samples = original.Samples,
                StartTime = original.StartTime,
                EndTime = original.GetEndTime()
            };

        private static SentakkiHitObject createTouchHold(HitObject original) => new TouchHold
        {
            StartTime = original.StartTime,
            EndTime = ((IHasDuration)original).EndTime,
            Samples = original.Samples,
        };

        private SentakkiHitObject createTouchNote(HitObject original)
        {
            var availableTouchPositions = VALID_TOUCH_POSITIONS.Where(v =>
            {
                if (endTimes.TryGetValue(v, out double endTime))
                    return original.StartTime >= endTime;
                return true;
            });

            // Choosing a position
            Vector2 position;
            if (availableTouchPositions.Any())
                position = availableTouchPositions.ElementAt(patternGenerator.RNG.Next(0, availableTouchPositions.Count()));
            else
                position = endTimes.OrderBy(x => x.Value).First().Key;

            endTimes[position] = original.StartTime + 500;

            return new Touch
            {
                Samples = original.Samples,
                StartTime = original.StartTime,
                Position = position
            };
        }

        private SentakkiHitObject createSlideNote(HitObject original, IList<IList<HitSampleInfo>> samples, bool twin = false, bool hasBreakHead = false, bool hasBreakTail = false)
        {
            int noteLane = patternGenerator.GetNextLane(twin);
            List<SlideBodyPart> validPaths;

            {
                var pathEnumerable = SlidePaths.VALIDPATHS.Where(p => ((IHasDuration)original).Duration >= p.MinDuration && ((IHasDuration)original).Duration <= p.MinDuration * 10)
                                                                                      .Select(t => t.parameters);

                if (!EnabledExperiments.HasFlag(ConversionExperiments.fanSlides))
                    pathEnumerable = pathEnumerable.Where(s => s.Shape != SlidePaths.PathShapes.Fan);

                validPaths = pathEnumerable.ToList();
            }

            if (!validPaths.Any()) return null!;
            var selectedPath = validPaths[patternGenerator.RNG.Next(validPaths.Count)];

            return new Slide
            {
                SlideInfoList = new List<SlideBodyInfo>{
                    new SlideBodyInfo
                    {
                        SlidePathParts = new SlideBodyPart[]{selectedPath},
                        Duration = ((IHasDuration)original).Duration,
                        Break = hasBreakTail,
                        ShootDelay = 0.5f,
                    }
                },
                Lane = noteLane,
                StartTime = original.StartTime,
                NodeSamples = samples,
                Break = hasBreakHead
            };
        }

        private IEnumerable<Tap> createTapsFromNodes(HitObject original, IList<IList<HitSampleInfo>> nodeSamples)
        {
            if (original is not IHasPathWithRepeats curve)
                yield break;

            double spanDuration = curve.Duration / (curve.RepeatCount + 1);
            bool isRepeatSpam = spanDuration < 75 && curve.RepeatCount > 0;

            if (isRepeatSpam)
                yield break;

            // There's a chance no taps will be generated from nodes
            if (patternGenerator.RNG.NextDouble() < 0.5)
                yield break;

            // There's a chance no taps will be generated from head nodes
            bool shouldConsiderStartTick = patternGenerator.RNG.NextDouble() < 0.5;

            // There's a chance no taps will be generated from head nodes
            // If the head node isn't considered, then neither will the tail
            bool shouldConsiderTailTick = shouldConsiderStartTick && patternGenerator.RNG.NextDouble() < 0.5;

            var difficulty = Beatmap.BeatmapInfo.Difficulty;

            var controlPointInfo = (LegacyControlPointInfo)Beatmap.ControlPointInfo;

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(original.StartTime);
            DifficultyControlPoint difficultyPoint = original.DifficultyControlPoint;

            double scoringDistance = 100 * difficulty.SliderMultiplier * difficultyPoint.SliderVelocity;

            double velocity = scoringDistance / timingPoint.BeatLength;
            double tickDistance = scoringDistance / difficulty.SliderTickRate;

            double legacyLastTickOffset = (original as IHasLegacyLastTickOffset)?.LegacyLastTickOffset ?? 0;

            int nodeSampleIndex = 0;

            foreach (var e in SliderEventGenerator.Generate(original.StartTime, spanDuration, velocity, tickDistance, curve.Path.Distance, curve.RepeatCount + 1, legacyLastTickOffset, CancellationToken.None))
            {
                switch (e.Type)
                {
                    case SliderEventType.Head:
                        if (shouldConsiderStartTick)
                            goto case SliderEventType.Repeat;

                        ++nodeSampleIndex;
                        break;

                    case SliderEventType.Tail:
                        if (shouldConsiderTailTick)
                            goto case SliderEventType.Repeat;

                        ++nodeSampleIndex;
                        break;

                    case SliderEventType.Repeat:
                        yield return new Tap
                        {
                            Lane = patternGenerator.GetNextLane(true),
                            Samples = nodeSamples[nodeSampleIndex],
                            StartTime = e.Time,
                            Break = nodeSamples[nodeSampleIndex].Any(s => s.Name == HitSampleInfo.HIT_FINISH)
                        };

                        ++nodeSampleIndex;
                        break;
                }
            }
        }

        #endregion
    }
}
