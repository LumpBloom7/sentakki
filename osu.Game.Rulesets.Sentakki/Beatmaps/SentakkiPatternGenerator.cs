using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using System.Diagnostics;
using System.Threading;

namespace osu.Game.Rulesets.Sentakki.Beatmaps
{
    public class SentakkiPatternGenerator
    {
        public Bindable<ConversionExperiments> Experiments = new Bindable<ConversionExperiments>();
        private readonly Random rng;

        private IBeatmap beatmap;
        public SentakkiPatternGenerator(IBeatmap beatmap)
        {
            this.beatmap = beatmap;
            var difficulty = beatmap.BeatmapInfo.BaseDifficulty;
            int seed = ((int)MathF.Round(difficulty.DrainRate + difficulty.CircleSize) * 20) + (int)(difficulty.OverallDifficulty * 41.2) + (int)MathF.Round(difficulty.ApproachRate);
            rng = new Random(seed);
        }

        //The patterns will generate the note path to be used based on the current offset
        // argument list is (offset, diff)
        private List<Func<bool, int>> patternlist => new List<Func<bool, int>>{
            //Stream pattern, path difference determined by offset2
            (twin)=> {
                if(twin) return offset + 4;
                else offset+=offset2;
                return offset;
            },
            // Back and forth, works better with longer combos
            // Path difference determined by offset2, but will make sure offset2 is never 0.
            (twin)=>{
                offset2 = offset2 == 0 ? 1 : offset2;
                offset+=offset2;
                offset2= -offset2;

                return offset;
            }
        };
        private int currentPattern = 0;
        private int offset = 0;
        private int offset2 = 0;

        private int getNewPath(bool twin = false) => patternlist[currentPattern].Invoke(twin);

        public void CreateNewPattern()
        {
            currentPattern = rng.Next(0, patternlist.Count); // Pick a pattern
            offset = rng.Next(0, 8); // Give it a random offset for variety
            offset2 = rng.Next(-2, 3); // Give it a random offset for variety
        }

        public IEnumerable<SentakkiHitObject> GenerateNewNote(HitObject original)
        {
            switch (original)
            {
                case IHasPathWithRepeats hold:
                    if (Experiments.Value.HasFlag(ConversionExperiments.twins))
                    {
                        if (hold.NodeSamples.Any(samples => samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP)))
                            yield return createHoldNote(original, true);
                        else
                            foreach (var note in createTapsFromTicks(original).ToList())
                                yield return note;
                    }

                    yield return createHoldNote(original);
                    break;

                case IHasDuration _:
                    yield return Conversions.CreateTouchHold(original);
                    break;

                default:
                    if (original.Samples.Any(s => s.Name == HitSampleInfo.HIT_FINISH))
                    {
                        if (Experiments.Value.HasFlag(ConversionExperiments.twins) && original.Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP))
                            yield return createTapNote<Break>(original, true);
                        yield return createTapNote<Break>(original);
                    }
                    else if (Experiments.Value.HasFlag(ConversionExperiments.touch) && original.Samples.Any(s => s.Name == HitSampleInfo.HIT_WHISTLE))
                    {
                        yield return createTouchNote(original);
                    }
                    else
                    {
                        if (Experiments.Value.HasFlag(ConversionExperiments.twins) && original.Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP))
                            yield return createTapNote(original, true);
                        yield return createTapNote(original);
                    }
                    break;
            }
        }

        // Individual note generation code, because it's cleaner
        private SentakkiHitObject createHoldNote(HitObject original, bool twin = false)
        {
            int notePath = getNewPath(twin);
            return new Hold
            {
                NoteColor = Color4.Crimson,
                Angle = notePath.GetAngleFromPath(),
                NodeSamples = (original as IHasPathWithRepeats).NodeSamples,
                StartTime = original.StartTime,
                EndTime = original.GetEndTime(),
                EndPosition = SentakkiExtensions.GetPosition(SentakkiPlayfield.INTERSECTDISTANCE, notePath),
                Position = SentakkiExtensions.GetPosition(SentakkiPlayfield.NOTESTARTDISTANCE, notePath),
            };
        }
        private IEnumerable<SentakkiHitObject> createTapsFromTicks(HitObject original)
        {
            int notePath = getNewPath(true);

            var curve = original as IHasPathWithRepeats;
            double spanDuration = curve.Duration / (curve.RepeatCount + 1);
            bool isRepeatSpam = spanDuration < 75 && curve.RepeatCount > 0;

            if (isRepeatSpam)
                yield break;

            var difficulty = beatmap.BeatmapInfo.BaseDifficulty;

            var controlPointInfo = beatmap.ControlPointInfo;
            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(original.StartTime);
            DifficultyControlPoint difficultyPoint = controlPointInfo.DifficultyPointAt(original.StartTime);

            double scoringDistance = 100 * difficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier;

            var velocity = scoringDistance / timingPoint.BeatLength;
            var tickDistance = scoringDistance / difficulty.SliderTickRate;

            double legacyLastTickOffset = (original as IHasLegacyLastTickOffset)?.LegacyLastTickOffset ?? 0;

            foreach (var e in SliderEventGenerator.Generate(original.StartTime, spanDuration, velocity, tickDistance, curve.Path.Distance, curve.RepeatCount + 1, legacyLastTickOffset, CancellationToken.None))
            {
                switch (e.Type)
                {
                    case SliderEventType.Tick:
                    case SliderEventType.Repeat:
                        yield return new Tap
                        {
                            NoteColor = Color4.Orange,
                            Angle = notePath.GetAngleFromPath(),
                            Samples = original.Samples.Select(s => new HitSampleInfo
                            {
                                Bank = s.Bank,
                                Name = @"slidertick",
                                Volume = s.Volume
                            }).ToList(),
                            StartTime = e.Time,
                            EndPosition = SentakkiExtensions.GetPosition(SentakkiPlayfield.INTERSECTDISTANCE, notePath),
                            Position = SentakkiExtensions.GetPosition(SentakkiPlayfield.NOTESTARTDISTANCE, notePath),
                        };
                        break;
                }
            }
        }

        private SentakkiHitObject createTapNote(HitObject original, bool twin = false) => createTapNote<Tap>(original, twin);
        private SentakkiHitObject createTapNote<T>(HitObject original, bool twin = false) where T : Tap, new()
        {
            int notePath = getNewPath(twin);
            return new T
            {
                NoteColor = (typeof(T) == typeof(Break)) ? Color4.OrangeRed : Color4.Orange,
                Angle = notePath.GetAngleFromPath(),
                Samples = original.Samples,
                StartTime = original.StartTime,
                EndPosition = SentakkiExtensions.GetPosition(SentakkiPlayfield.INTERSECTDISTANCE, notePath),
                Position = SentakkiExtensions.GetPosition(SentakkiPlayfield.NOTESTARTDISTANCE, notePath),
            };
        }

        private SentakkiHitObject createTouchNote(HitObject original)
        {
            Vector2 newPos = (original as IHasPosition)?.Position ?? Vector2.Zero;
            newPos = new Vector2((newPos.X / 512 * 400) - 200, (newPos.Y / 384 * 400) - 200);
            return new Touch
            {
                NoteColor = Color4.Cyan,
                Samples = original.Samples,
                StartTime = original.StartTime,
                Position = newPos
            };
        }
    }
}