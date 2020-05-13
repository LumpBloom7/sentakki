
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using System;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Objects;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osuTK.Graphics;
using System.Linq;

namespace osu.Game.Rulesets.Sentakki
{
    public static class SentakkiExtensions
    {
        public static Vector2 GetPosition(float distance, float angle)
        {
            return new Vector2(-(distance * (float)Math.Cos((angle + 90f) * (float)(Math.PI / 180))), -(distance * (float)Math.Sin((angle + 90f) * (float)(Math.PI / 180))));
        }

        public static float GetDegreesFromPosition(this Vector2 target, Vector2 self)
        {
            return (float)MathHelper.RadiansToDegrees(Math.Atan2(target.X - self.X, target.Y - self.Y));
        }

        public static float GetNotePathFromDegrees(this float degrees)
        {
            if (degrees < 0) degrees += 360;
            if (degrees >= 360) degrees %= 360;
            int result = 0;

            for (int i = 0; i < SentakkiPlayfield.PATHANGLES.Length; ++i)
            {
                if (SentakkiPlayfield.PATHANGLES[i] - degrees >= -22.5f && SentakkiPlayfield.PATHANGLES[i] - degrees <= 22.5f)
                    result = i;
            }
            return SentakkiPlayfield.PATHANGLES[result];
        }

        public static List<SentakkiHitObject> CreateTapFromTicks(HitObject obj, IBeatmap beatmap, IHasCurve curve, Random rng)
        {
            double spanDuration = curve.Duration / (curve.RepeatCount + 1);
            bool isRepeatSpam = spanDuration < 75 && curve.RepeatCount > 0;

            Vector2 CENTRE_POINT = new Vector2(256, 192);
            List<SentakkiHitObject> hitObjects = new List<SentakkiHitObject>();
            if (isRepeatSpam)
                return hitObjects;

            var difficulty = beatmap.BeatmapInfo.BaseDifficulty;

            var objPosition = (obj as IHasPosition)?.Position ?? Vector2.Zero;

            var controlPointInfo = beatmap.ControlPointInfo;
            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(obj.StartTime);
            DifficultyControlPoint difficultyPoint = controlPointInfo.DifficultyPointAt(obj.StartTime);

            double scoringDistance = 100 * difficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier;

            var velocity = scoringDistance / timingPoint.BeatLength;
            var tickDistance = scoringDistance / difficulty.SliderTickRate;

            double legacyLastTickOffset = (obj as IHasLegacyLastTickOffset)?.LegacyLastTickOffset ?? 0;

            foreach (var e in SliderEventGenerator.Generate(obj.StartTime, spanDuration, velocity, tickDistance, curve.Path.Distance, curve.RepeatCount + 1, legacyLastTickOffset))
            {

                var sliderEventPosition = (curve.CurvePositionAt(e.PathProgress / (curve.RepeatCount + 1)) + objPosition) * new Vector2(1, 0.5f);
                sliderEventPosition.Y = 384 - sliderEventPosition.Y;

                float angle;
                switch (e.Type)
                {
                    case SliderEventType.Tick:
                    case SliderEventType.Repeat:
                        hitObjects.Add(new Tap
                        {
                            NoteColor = Color4.Orange,
                            Angle = angle = (sliderEventPosition.GetDegreesFromPosition(CENTRE_POINT) + (22.5f * rng.Next(1, 6))).GetNotePathFromDegrees(),
                            Samples = getTickSamples(obj.Samples),
                            StartTime = e.Time,
                            EndPosition = SentakkiExtensions.GetPosition(SentakkiPlayfield.INTERSECTDISTANCE, angle),
                            Position = SentakkiExtensions.GetPosition(SentakkiPlayfield.NOTESTARTDISTANCE, angle),
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
