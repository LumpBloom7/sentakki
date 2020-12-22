using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Slide : SentakkiLanedHitObject, IHasDuration
    {
        protected override bool NeedBreakSample => false;

        public double Duration
        {
            get => SlideInfoList.Any() ? SlideInfoList.Max(s => s.Duration) : 0;
            set => throw new NotSupportedException();
        }

        public List<IList<HitSampleInfo>> NodeSamples = new List<IList<HitSampleInfo>>();

        public double EndTime => StartTime + Duration;

        protected override Color4 DefaultNoteColour => Color4.Aqua;

        public List<SentakkiSlideInfo> SlideInfoList = new List<SentakkiSlideInfo>();

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            AddNested(new SlideTap
            {
                HasTwin = HasTwin || SlideInfoList.Count > 1,
                LaneBindable = { BindTarget = LaneBindable },
                StartTime = StartTime,
                Samples = NodeSamples.Any() ? NodeSamples.First() : new List<HitSampleInfo>(),
                Break = Break
            });
            createSlideBodies();
        }

        private void createSlideBodies()
        {
            foreach (var SlideInfo in SlideInfoList)
            {
                AddNested(new SlideBody
                {
                    HasTwin = HasTwin || SlideInfoList.Count > 1,
                    Lane = SlideInfo.SlidePath.EndLane + Lane,
                    StartTime = StartTime,
                    SlideInfo = SlideInfo,
                    Samples = NodeSamples.Any() ? NodeSamples.Last() : new List<HitSampleInfo>(),
                });
            }
        }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
        public override Judgement CreateJudgement() => new IgnoreJudgement();
    }
}
