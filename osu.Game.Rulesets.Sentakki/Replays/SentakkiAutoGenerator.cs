using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Replays;
using osuTK;
using System;
using System.Linq;

namespace osu.Game.Rulesets.Sentakki.Replays
{
    public class SentakkiAutoGenerator : AutoGenerator
    {
        public const double KEY_RELEASE_DELAY = 20;
        public const double TOUCH_RELEASE_DELAY = 50;
        public const double TOUCH_REACT_DELAY = 200;

        protected Replay Replay;
        protected List<ReplayFrame> Frames => Replay.Frames;

        public new Beatmap<SentakkiHitObject> Beatmap => (Beatmap<SentakkiHitObject>)base.Beatmap;

        public SentakkiAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
            Replay = new Replay();
        }

        public override Replay Generate()
        {
            //add some frames at the beginning so the cursor doesnt suddenly appear on the first note
            Frames.Add(new SentakkiReplayFrame { Position = new Vector2(-1000), Time = -500 });

            var pointGroups = generateActionPoints().GroupBy(a => a.Time).OrderBy(g => g.First().Time);

            var actions = new List<SentakkiAction>();
            var TouchReplayEvents = new TouchReplayEvent[10];

            foreach (var group in pointGroups)
            {
                foreach (var point in group)
                {
                    switch (point)
                    {
                        case KeyDown e:
                            actions.Add(SentakkiAction.Key1 + e.Lane);
                            break;

                        case KeyUp e:
                            actions.Remove(SentakkiAction.Key1 + e.Lane);
                            break;
                        case TouchDown e:
                            TouchReplayEvents[e.PointNumber] = e.TouchReplayEvent;
                            break;
                        case TouchUp e:
                            TouchReplayEvents[e.PointNumber] = null;
                            break;
                    }
                }

                // todo: can be removed once FramedReplayInputHandler correctly handles rewinding before first frame.
                if (Replay.Frames.Count == 0)
                    Replay.Frames.Add(new SentakkiReplayFrame(group.First().Time - 1, new Vector2(-1000), false, Array.Empty<TouchReplayEvent>()));

                Replay.Frames.Add(new SentakkiReplayFrame(group.First().Time, new Vector2(-1000), false, TouchReplayEvents.ToList().ToArray(), actions.ToArray()));
            }

            return Replay;
        }

        private IEnumerable<IActionPoint> generateActionPoints()
        {
            var touchPointInUsedUntil = new double?[10];

            for (int i = 0; i < Beatmap.HitObjects.Count; i++)
            {
                var currentObject = Beatmap.HitObjects[i];
                double endTime = currentObject.GetEndTime();

                int nextAvailableTouchPoint() => Array.IndexOf(touchPointInUsedUntil, touchPointInUsedUntil.First(t => !t.HasValue || t.Value < currentObject.StartTime));

                switch (currentObject)
                {
                    case SentakkiLanedHitObject laned:
                        var nextObjectInColumn = GetNextObject(i) as SentakkiLanedHitObject; // Get the next object that requires pressing the same button

                        bool canDelayKeyUp = nextObjectInColumn == null ||
                                             nextObjectInColumn.StartTime > endTime + KEY_RELEASE_DELAY;

                        double calculatedDelay = canDelayKeyUp ? KEY_RELEASE_DELAY : (nextObjectInColumn.StartTime - endTime) * 0.9;

                        yield return new TouchDown
                        {
                            Time = currentObject.StartTime,
                            TouchReplayEvent = new TouchReplayEvent(SentakkiExtensions.GetCircularPosition(295.5f, laned.Lane.GetRotationForLane()), 0, currentObject.StartTime),
                            PointNumber = nextAvailableTouchPoint()
                        };
                        yield return new KeyUp { Time = endTime + calculatedDelay, Lane = laned.Lane };

                        if (laned is Slide s)
                        {
                            foreach (var slideInfo in s.SlideInfoList)
                            {
                                double delay = Beatmap.ControlPointInfo.TimingPointAt(currentObject.StartTime).BeatLength * slideInfo.ShootDelay / 2;
                                if (delay >= slideInfo.Duration - 50) delay = 0;
                                yield return new TouchDown
                                {
                                    Time = currentObject.StartTime + delay,
                                    TouchReplayEvent = new TouchReplayEvent(slideInfo.SlidePath.Path, slideInfo.Duration - delay, currentObject.StartTime + delay, s.Lane.GetRotationForLane() - 22.5f),
                                    PointNumber = nextAvailableTouchPoint()
                                };
                                yield return new TouchUp { Time = currentObject.StartTime + slideInfo.Duration + TOUCH_RELEASE_DELAY, PointNumber = nextAvailableTouchPoint() };
                                touchPointInUsedUntil[nextAvailableTouchPoint()] = currentObject.StartTime + slideInfo.Duration + TOUCH_REACT_DELAY;
                            }
                        }
                        else
                        {
                            yield return new TouchUp { Time = currentObject.GetEndTime() + TOUCH_RELEASE_DELAY, PointNumber = nextAvailableTouchPoint() };
                            touchPointInUsedUntil[nextAvailableTouchPoint()] = currentObject.GetEndTime() + TOUCH_REACT_DELAY;
                        }


                        break;

                    case Touch _:
                    case TouchHold _:
                        yield return new TouchDown
                        {
                            Time = currentObject.StartTime,
                            PointNumber = nextAvailableTouchPoint(),
                            TouchReplayEvent = new TouchReplayEvent((currentObject is Touch x) ? x.Position : Vector2.Zero, TOUCH_RELEASE_DELAY, currentObject.StartTime)
                        };
                        yield return new TouchUp { Time = endTime + TOUCH_RELEASE_DELAY, PointNumber = nextAvailableTouchPoint() };
                        touchPointInUsedUntil[nextAvailableTouchPoint()] = endTime + TOUCH_REACT_DELAY;
                        break;
                }
            }
        }

        protected override HitObject GetNextObject(int currentIndex)
        {
            int desiredLane = (Beatmap.HitObjects[currentIndex] as SentakkiLanedHitObject).Lane;

            for (int i = currentIndex + 1; i < Beatmap.HitObjects.Count; i++)
            {
                if (Beatmap.HitObjects[i] is SentakkiLanedHitObject laned && laned.Lane == desiredLane)
                    return Beatmap.HitObjects[i];
            }

            return null;
        }

        private interface IActionPoint
        {
            double Time { get; set; }
        }
        private interface ILanedActionPoint : IActionPoint
        {
            int Lane { get; set; }
        }
        private interface ITouchActionPoint : IActionPoint
        {
            TouchReplayEvent TouchReplayEvent { get; set; }
            int PointNumber { get; set; }
        }

        private struct KeyDown : ILanedActionPoint
        {
            public double Time { get; set; }
            public int Lane { get; set; }
        }

        private struct KeyUp : ILanedActionPoint
        {
            public double Time { get; set; }
            public int Lane { get; set; }
        }
        private struct TouchDown : ITouchActionPoint
        {
            public double Time { get; set; }
            public TouchReplayEvent TouchReplayEvent { get; set; }
            public int PointNumber { get; set; }
        }
        private struct TouchUp : ITouchActionPoint
        {
            public double Time { get; set; }
            public TouchReplayEvent TouchReplayEvent { get; set; }
            public int PointNumber { get; set; }
        }
    }
}
