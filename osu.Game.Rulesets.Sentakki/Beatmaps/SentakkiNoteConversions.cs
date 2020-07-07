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
    public static class Conversions
    {
        public static List<SentakkiHitObject> CreateTapNote(HitObject original, int path, Random rng, ConversionExperiments experimental = ConversionExperiments.none)
        {
            List<SentakkiHitObject> notes = new List<SentakkiHitObject>();
            bool strong = original.Samples.Any(s => s.Name == HitSampleInfo.HIT_FINISH);
            bool twin = original.Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP);

            notes.Add(new Tap
            {
                IsBreak = strong,
                Path = path,
                Samples = original.Samples,
                StartTime = original.StartTime
            });
            if (twin && experimental.HasFlag(ConversionExperiments.twins))
            {
                int newPath = path;
                while (path == newPath) newPath = rng.Next(0, 8);
                notes.Add(new Tap
                {
                    IsBreak = strong,
                    Path = newPath,
                    Samples = original.Samples,
                    StartTime = original.StartTime
                });
                foreach (var note in notes)
                    note.HasTwin = true;
            }

            return notes;
        }

        public static List<SentakkiHitObject> CreateTouchNote(HitObject original, int path, Random rng, ConversionExperiments experimental = ConversionExperiments.none)
        {
            Vector2 newPos = (original as IHasPosition)?.Position ?? Vector2.Zero;
            newPos.Y = 384 - newPos.Y - 192;
            newPos.X = (newPos.X - 256) * 0.6f;

            List<SentakkiHitObject> notes = new List<SentakkiHitObject>{new Touch
            {
                Samples = original.Samples,
                StartTime = original.StartTime,
                Position = newPos
            }};

            return notes;
        }

        public static SentakkiHitObject CreateTouchHold(HitObject original)
        => new TouchHold
        {

            StartTime = original.StartTime,
            EndTime = (original as IHasDuration).EndTime,
            Samples = original.Samples,
        };

        public static List<SentakkiHitObject> CreateHoldNote(HitObject original, int path, IBeatmap beatmap, Random rng, ConversionExperiments experimental = ConversionExperiments.none)
        {
            var curveData = original as IHasPathWithRepeats;

            List<SentakkiHitObject> notes = new List<SentakkiHitObject>();
            bool twin = curveData.NodeSamples.Any(s => s.Any(s => s.Name == HitSampleInfo.HIT_CLAP));
            bool strong = curveData.NodeSamples.Any(s => s.Any(s => s.Name == HitSampleInfo.HIT_FINISH));

            notes.Add(new Hold
            {
                IsBreak = strong,
                Path = path,
                NodeSamples = curveData.NodeSamples,
                StartTime = original.StartTime,
                EndTime = original.GetEndTime()
            });

            if (experimental.HasFlag(ConversionExperiments.twins))
            {
                if (twin)
                {
                    int newPath = path;
                    while (path == newPath) newPath = rng.Next(0, 8);
                    notes.Add(new Hold
                    {
                        IsBreak = strong,
                        Path = newPath,
                        NodeSamples = curveData.NodeSamples,
                        StartTime = original.StartTime,
                        EndTime = original.GetEndTime(),
                    });
                    foreach (var note in notes)
                        note.HasTwin = true;
                }
                else
                {
                    var taps = CreateTapFromTicks(original, path, beatmap, rng);
                    if (taps.Any())
                        notes.AddRange(taps);
                }
            }

            return notes;
        }

        public static List<SentakkiHitObject> CreateTapFromTicks(HitObject original, int path, IBeatmap beatmap, Random rng)
        {
            var curve = original as IHasPathWithRepeats;
            double spanDuration = curve.Duration / (curve.RepeatCount + 1);
            bool isRepeatSpam = spanDuration < 75 && curve.RepeatCount > 0;

            List<SentakkiHitObject> hitObjects = new List<SentakkiHitObject>();
            if (isRepeatSpam)
                return hitObjects;

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
                int newPath = path;
                while (newPath == path) newPath = rng.Next(0, 8);

                switch (e.Type)
                {
                    case SliderEventType.Tick:
                    case SliderEventType.Repeat:
                        hitObjects.Add(new Tap
                        {
                            Path = newPath,
                            Samples = getTickSamples(original.Samples),
                            StartTime = e.Time,
                        });
                        break;
                }
            }
            return hitObjects;
        }

        private static List<HitSampleInfo> getTickSamples(IList<HitSampleInfo> objSamples) => objSamples.Select(s => new HitSampleInfo
        {
            Bank = s.Bank,
            Name = @"slidertick",
            Volume = s.Volume
        }).ToList();
    }
}
