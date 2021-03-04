using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.Scoring;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class SlideBody : SentakkiLanedHitObject, IHasDuration
    {
        protected override Color4 DefaultNoteColour => Color4.Aqua;

        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public double Duration
        {
            get => SlideInfo.Duration;
            set => SlideInfo.Duration = value;
        }

        public SentakkiSlideInfo SlideInfo { get; set; }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            bool isSampleAdded = false;
            foreach (var (segment,start,end) in SlideVisual.CreateSegmentsFor(SlideInfo.SlidePath.Path))
            {
                var count = segment.Count();
                var left = count;
                int i = 0;
                foreach (var (pos, rot) in segment)
                {
                    // skip first node, insert each third chevron unless there are less than 2 left (if so, insert one at the end)
                    if(i != 0 && ((i % 3 == 0 && left >= 3) || left == 1))
                    {
                        var progress = (double)i / (count - 1);
                        progress = start + (progress * (end - start));

                        SlideNode node;
                        AddNested(node = new SlideNode {
                            StartTime = StartTime + ShootDelay + ((Duration - ShootDelay) * progress),
                            Progress = (float)progress
                        });

                        if (!isSampleAdded)
                        {
                            isSampleAdded = true;
                            node.Samples.Add(new SentakkiHitSampleInfo("slide"));
                        }
                    }

                    i++;
                    left--;
                }
            }
        }

        [JsonIgnore]
        public double ShootDelay { get; private set; }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            double delay = controlPointInfo.TimingPointAt(StartTime).BeatLength * SlideInfo.ShootDelay / 2;
            if (delay < Duration - 50)
                ShootDelay = delay;
        }

        protected override HitWindows CreateHitWindows() => new SentakkiSlideHitWindows();
        public override Judgement CreateJudgement() => new SentakkiJudgement();

        public class SlideNode : SentakkiHitObject
        {
            public virtual float Progress { get; set; }

            protected override HitWindows CreateHitWindows() => HitWindows.Empty;
            public override Judgement CreateJudgement() => new IgnoreJudgement();
        }
    }
}
