using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Replays;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Sentakki.Replays
{
    public class SentakkiAutoGenerator : AutoGenerator
    {
        public const double RELEASE_DELAY = 20;

        protected Replay Replay;
        protected List<ReplayFrame> Frames => Replay.Frames;

        public new Beatmap<SentakkiHitObject> Beatmap => (Beatmap<SentakkiHitObject>)base.Beatmap;

        private List<SentakkiHitObject> lanedHitObjects;

        public SentakkiAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
            Replay = new Replay();
        }

        public override Replay Generate()
        {
            //add some frames at the beginning so the cursor doesnt suddenly appear on the first note
            Frames.Add(new SentakkiReplayFrame { Position = new Vector2(-1000), Time = -500 });

            lanedHitObjects = Beatmap.HitObjects.Where(h => h is SentakkiLanedHitObject).ToList();

            var pointGroups = generateActionPoints().GroupBy(a => a.Time).OrderBy(g => g.First().Time);

            var actions = new List<SentakkiAction>();

            foreach (var group in pointGroups)
            {
                foreach (var point in group)
                {
                    switch (point)
                    {
                        case HitPoint _:
                            actions.Add(SentakkiAction.Key1 + point.Lane);
                            break;

                        case ReleasePoint _:
                            actions.Remove(SentakkiAction.Key1 + point.Lane);
                            break;
                    }
                }

                // todo: can be removed once FramedReplayInputHandler correctly handles rewinding before first frame.
                if (Replay.Frames.Count == 0)
                    Replay.Frames.Add(new SentakkiReplayFrame(group.First().Time - 1, new Vector2(-1000)));

                Replay.Frames.Add(new SentakkiReplayFrame(group.First().Time, new Vector2(-1000), actions.ToArray()));
            }

            return Replay;
        }

        private IEnumerable<IActionPoint> generateActionPoints()
        {
            for (int i = 0; i < lanedHitObjects.Count; i++)
            {
                var currentObject = lanedHitObjects[i] as SentakkiLanedHitObject;
                var nextObjectInColumn = GetNextObject(i) as SentakkiLanedHitObject; // Get the next object that requires pressing the same button

                double endTime = currentObject.GetEndTime();

                bool canDelayKeyUp = nextObjectInColumn == null ||
                                     nextObjectInColumn.StartTime > endTime + RELEASE_DELAY;

                double calculatedDelay = canDelayKeyUp ? RELEASE_DELAY : (nextObjectInColumn.StartTime - endTime) * 0.9;

                yield return new HitPoint { Time = currentObject.StartTime, Lane = currentObject.Lane };

                yield return new ReleasePoint { Time = endTime + calculatedDelay, Lane = currentObject.Lane };
            }
        }

        protected override HitObject GetNextObject(int currentIndex)
        {
            int desiredLane = (lanedHitObjects[currentIndex] as SentakkiLanedHitObject).Lane;

            for (int i = currentIndex + 1; i < lanedHitObjects.Count; i++)
            {
                if ((lanedHitObjects[i] as SentakkiLanedHitObject).Lane == desiredLane)
                    return lanedHitObjects[i];
            }

            return null;
        }

        private interface IActionPoint
        {
            double Time { get; set; }
            int Lane { get; set; }
        }

        private struct HitPoint : IActionPoint
        {
            public double Time { get; set; }
            public int Lane { get; set; }
        }

        private struct ReleasePoint : IActionPoint
        {
            public double Time { get; set; }
            public int Lane { get; set; }
        }
    }
}
