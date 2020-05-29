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

namespace osu.Game.Rulesets.Sentakki.Beatmaps
{
    public static class Conversions
    {
        public static List<SentakkiHitObject> CreateTapNote(HitObject original, int path, Random rng, bool experimental = false)
        {
            List<SentakkiHitObject> notes = new List<SentakkiHitObject>();
            bool strong = original.Samples.Any(s => s.Name == HitSampleInfo.HIT_FINISH);
            bool twin = original.Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP);

            if (strong)
            {
                notes.Add(new Break
                {
                    NoteColor = Color4.OrangeRed,
                    Angle = path.GetAngleFromPath(),
                    Samples = original.Samples,
                    StartTime = original.StartTime,
                    EndPosition = SentakkiExtensions.GetPosition(SentakkiPlayfield.INTERSECTDISTANCE, path),
                    Position = SentakkiExtensions.GetPosition(SentakkiPlayfield.NOTESTARTDISTANCE, path),
                });
                if (twin && experimental)
                {
                    int newPath = path;
                    while (path == newPath) newPath = rng.Next(0, 8);
                    notes.Add(new Break
                    {
                        NoteColor = Color4.OrangeRed,
                        Angle = newPath.GetAngleFromPath(),
                        Samples = original.Samples,
                        StartTime = original.StartTime,
                        EndPosition = SentakkiExtensions.GetPosition(SentakkiPlayfield.INTERSECTDISTANCE, newPath),
                        Position = SentakkiExtensions.GetPosition(SentakkiPlayfield.NOTESTARTDISTANCE, newPath),
                    });
                }
            }
            else
            {
                notes.Add(new Tap
                {
                    NoteColor = Color4.Orange,
                    Angle = path.GetAngleFromPath(),
                    Samples = original.Samples,
                    StartTime = original.StartTime,
                    EndPosition = SentakkiExtensions.GetPosition(SentakkiPlayfield.INTERSECTDISTANCE, path),
                    Position = SentakkiExtensions.GetPosition(SentakkiPlayfield.NOTESTARTDISTANCE, path),
                });
                if (twin && experimental)
                {
                    int newPath = path;
                    while (path == newPath) newPath = rng.Next(0, 8);
                    notes.Add(new Tap
                    {
                        NoteColor = Color4.Orange,
                        Angle = newPath.GetAngleFromPath(),
                        Samples = original.Samples,
                        StartTime = original.StartTime,
                        EndPosition = SentakkiExtensions.GetPosition(SentakkiPlayfield.INTERSECTDISTANCE, newPath),
                        Position = SentakkiExtensions.GetPosition(SentakkiPlayfield.NOTESTARTDISTANCE, newPath),
                    });
                }
            }
            return notes;
        }

        public static List<SentakkiHitObject> CreateTouchNote(HitObject original, int path, Random rng, bool experimental = false)
        {
            Vector2 newPos = (original as IHasPosition)?.Position ?? Vector2.Zero;
            newPos.Y = (384 - newPos.Y) - 192;
            newPos.X = (-newPos.X) - 256;

            List<SentakkiHitObject> notes = new List<SentakkiHitObject>{new Touch
            {
                Samples = original.Samples,
                StartTime = original.StartTime,
                Position = newPos,
            }};

            return notes;
        }


        public static SentakkiHitObject CreateTouchHold(HitObject original)
        => new TouchHold
        {
            Position = Vector2.Zero,
            StartTime = original.StartTime,
            EndTime = (original as IHasEndTime).EndTime,
            Samples = original.Samples,
        };

        public static List<SentakkiHitObject> CreateHoldNote(HitObject original, int path, IBeatmap beatmap, Random rng, bool experimental)
        {
            var curveData = original as IHasCurve;

            List<SentakkiHitObject> notes = new List<SentakkiHitObject>();
            bool twin = curveData.NodeSamples.Any(s => s.Any(s => s.Name == HitSampleInfo.HIT_CLAP));

            notes.Add(new Hold
            {
                NoteColor = Color4.Crimson,
                Angle = path.GetAngleFromPath(),
                NodeSamples = curveData.NodeSamples,
                StartTime = original.StartTime,
                EndTime = original.GetEndTime(),
                EndPosition = SentakkiExtensions.GetPosition(SentakkiPlayfield.INTERSECTDISTANCE, path),
                Position = SentakkiExtensions.GetPosition(SentakkiPlayfield.NOTESTARTDISTANCE, path),
            });

            if (experimental)
            {
                if (twin)
                {
                    int newPath = path;
                    while (path == newPath) newPath = rng.Next(0, 8);
                    notes.Add(new Hold
                    {
                        NoteColor = Color4.Crimson,
                        Angle = newPath.GetAngleFromPath(),
                        NodeSamples = curveData.NodeSamples,
                        StartTime = original.StartTime,
                        EndTime = original.GetEndTime(),
                        EndPosition = SentakkiExtensions.GetPosition(SentakkiPlayfield.INTERSECTDISTANCE, newPath),
                        Position = SentakkiExtensions.GetPosition(SentakkiPlayfield.NOTESTARTDISTANCE, newPath),
                    });
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
            var curve = original as IHasCurve;
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

            foreach (var e in SliderEventGenerator.Generate(original.StartTime, spanDuration, velocity, tickDistance, curve.Path.Distance, curve.RepeatCount + 1, legacyLastTickOffset))
            {
                int newPath = path;
                while (newPath == path) newPath = rng.Next(0, 8);

                switch (e.Type)
                {
                    case SliderEventType.Tick:
                    case SliderEventType.Repeat:
                        hitObjects.Add(new Tap
                        {
                            NoteColor = Color4.Orange,
                            Angle = newPath.GetAngleFromPath(),
                            Samples = getTickSamples(original.Samples),
                            StartTime = e.Time,
                            EndPosition = SentakkiExtensions.GetPosition(SentakkiPlayfield.INTERSECTDISTANCE, newPath),
                            Position = SentakkiExtensions.GetPosition(SentakkiPlayfield.NOTESTARTDISTANCE, newPath),
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
