// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.IEnumerableExtensions;
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

namespace osu.Game.Rulesets.Sentakki.Beatmaps
{
    public class SentakkiBeatmapConverter : BeatmapConverter<SentakkiHitObject>
    {
        // todo: Check for conversion types that should be supported (ie. Beatmap.HitObjects.Any(h => h is IHasXPosition))
        // https://github.com/ppy/osu/tree/master/osu.Game/Rulesets/Objects/Types
        public override bool CanConvert() => Beatmap.HitObjects.All(h => h is IHasPosition);
        public bool Experimental = false;

        private Random random;

        public SentakkiBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
            var difficulty = beatmap.BeatmapInfo.BaseDifficulty;
            int seed = ((int)MathF.Round(difficulty.DrainRate + difficulty.CircleSize) * 20) + (int)(difficulty.OverallDifficulty * 41.2) + (int)MathF.Round(difficulty.ApproachRate);
            random = new Random(seed);
        }

        protected override IEnumerable<SentakkiHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap)
        {
            Vector2 CENTRE_POINT = new Vector2(256, 192);
            Vector2 newPos = (original as IHasPosition)?.Position ?? Vector2.Zero;
            newPos.Y = 384 - newPos.Y;
            float angle = Utils.GetNotePathFromDegrees(Utils.GetDegreesFromPosition(newPos, CENTRE_POINT));

            bool twinNote = original.Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP);
            List<SentakkiHitObject> objects = new List<SentakkiHitObject>();

            switch (original)
            {
                case IHasCurve curveData:
                    twinNote = curveData.NodeSamples.Any(s => s.Any(s => s.Name == HitSampleInfo.HIT_CLAP));
                    objects.Add(new Hold
                    {
                        NoteColor = Color4.Crimson,
                        Angle = Utils.GetNotePathFromDegrees(Utils.GetDegreesFromPosition(newPos, CENTRE_POINT)),
                        NodeSamples = curveData.NodeSamples,
                        StartTime = original.StartTime,
                        EndTime = original.GetEndTime(),
                        EndPosition = new Vector2(-(SentakkiPlayfield.INTERSECTDISTANCE * (float)Math.Cos((angle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.INTERSECTDISTANCE * (float)Math.Sin((angle + 90f) * (float)(Math.PI / 180)))),
                        Position = new Vector2(-(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Cos((angle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Sin((angle + 90f) * (float)(Math.PI / 180)))),
                    });
                    if (twinNote && Experimental)
                        objects.Add(new Hold
                        {
                            NoteColor = Color4.Crimson,
                            Angle = angle = Utils.GetNotePathFromDegrees(Utils.GetDegreesFromPosition(newPos, CENTRE_POINT) + (22.5f * random.Next(1, 7))),
                            NodeSamples = curveData.NodeSamples,
                            StartTime = original.StartTime,
                            EndTime = original.GetEndTime(),
                            EndPosition = new Vector2(-(SentakkiPlayfield.INTERSECTDISTANCE * (float)Math.Cos((angle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.INTERSECTDISTANCE * (float)Math.Sin((angle + 90f) * (float)(Math.PI / 180)))),
                            Position = new Vector2(-(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Cos((angle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Sin((angle + 90f) * (float)(Math.PI / 180)))),
                        });
                    break;

                case IHasEndTime endTimeData:
                    objects.Add(new TouchHold
                    {
                        Position = Vector2.Zero,
                        StartTime = original.StartTime,
                        EndTime = endTimeData.EndTime,
                        Samples = original.Samples,
                    });
                    break;

                default:
                    bool strong = original.Samples.Any(s => s.Name == HitSampleInfo.HIT_FINISH);
                    if (strong)
                    {
                        objects.Add(new Break
                        {
                            NoteColor = Color4.OrangeRed,
                            Angle = Utils.GetNotePathFromDegrees(Utils.GetDegreesFromPosition(newPos, CENTRE_POINT)),
                            Samples = original.Samples,
                            StartTime = original.StartTime,
                            EndPosition = new Vector2(-(SentakkiPlayfield.INTERSECTDISTANCE * (float)Math.Cos((angle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.INTERSECTDISTANCE * (float)Math.Sin((angle + 90f) * (float)(Math.PI / 180)))),
                            Position = new Vector2(-(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Cos((angle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Sin((angle + 90f) * (float)(Math.PI / 180)))),
                        });
                        if (twinNote && Experimental)
                            objects.Add(new Break
                            {
                                NoteColor = Color4.OrangeRed,
                                Angle = angle = Utils.GetNotePathFromDegrees(Utils.GetDegreesFromPosition(newPos, CENTRE_POINT) + (22.5f * random.Next(1, 7))),
                                Samples = original.Samples,
                                StartTime = original.StartTime,
                                EndPosition = new Vector2(-(SentakkiPlayfield.INTERSECTDISTANCE * (float)Math.Cos((angle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.INTERSECTDISTANCE * (float)Math.Sin((angle + 90f) * (float)(Math.PI / 180)))),
                                Position = new Vector2(-(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Cos((angle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Sin((angle + 90f) * (float)(Math.PI / 180)))),
                            });
                    }
                    else
                    {
                        objects.Add(new Tap
                        {
                            NoteColor = Color4.Orange,
                            Angle = Utils.GetNotePathFromDegrees(Utils.GetDegreesFromPosition(newPos, CENTRE_POINT)),
                            Samples = original.Samples,
                            StartTime = original.StartTime,
                            EndPosition = new Vector2(-(SentakkiPlayfield.INTERSECTDISTANCE * (float)Math.Cos((angle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.INTERSECTDISTANCE * (float)Math.Sin((angle + 90f) * (float)(Math.PI / 180)))),
                            Position = new Vector2(-(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Cos((angle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Sin((angle + 90f) * (float)(Math.PI / 180)))),
                        });
                        if (twinNote && Experimental)
                            objects.Add(new Tap
                            {
                                NoteColor = Color4.Orange,
                                Angle = angle = Utils.GetNotePathFromDegrees(Utils.GetDegreesFromPosition(newPos, CENTRE_POINT) + (22.5f * random.Next(1, 7))),
                                Samples = original.Samples,
                                StartTime = original.StartTime,
                                EndPosition = new Vector2(-(SentakkiPlayfield.INTERSECTDISTANCE * (float)Math.Cos((angle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.INTERSECTDISTANCE * (float)Math.Sin((angle + 90f) * (float)(Math.PI / 180)))),
                                Position = new Vector2(-(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Cos((angle + 90f) * (float)(Math.PI / 180))), -(SentakkiPlayfield.NOTESTARTDISTANCE * (float)Math.Sin((angle + 90f) * (float)(Math.PI / 180)))),
                            });
                    }
                    break;
            }
            foreach (var hitObject in objects)
                yield return hitObject;

            yield break;
        }

        protected override Beatmap<SentakkiHitObject> CreateBeatmap() => new SentakkiBeatmap();
    }
}
