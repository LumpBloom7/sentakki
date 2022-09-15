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
            get
            {
                double max = 0;
                for (int i = 0; i < SlideInfoList.Count; ++i)
                    max = Math.Max(max, SlideInfoList[i].Duration);

                return max;
            }
            set => throw new NotSupportedException();
        }

        public IList<IList<HitSampleInfo>> NodeSamples = new List<IList<HitSampleInfo>>();

        public double EndTime => StartTime + Duration;

        public override Color4 DefaultNoteColour => Color4.Aqua;

        public List<SentakkiSlideInfo> SlideInfoList = new List<SentakkiSlideInfo>();

        public SlideTap SlideTap { get; private set; }

        public IList<SlideBody> SlideBodies { get; private set; }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            AddNested(SlideTap = new SlideTap
            {
                LaneBindable = { BindTarget = LaneBindable },
                StartTime = StartTime,
                Samples = NodeSamples.Any() ? NodeSamples.First() : new List<HitSampleInfo>(),
                Break = Break
            });
            createSlideBodies();
        }

        private void createSlideBodies()
        {
            SlideBodies = new List<SlideBody>();

            foreach (var SlideInfo in SlideInfoList)
            {
                SlideBody body;
                if (SlideInfo.PathParameters[0].Shape == SlidePaths.PathShapes.Fan)
                    AddNested(body = new SlideFan());
                else
                    AddNested(body = new SlideBody());

                SlideBodies.Add(body);

                body.Lane = SlideInfo.SlidePath.EndLane + Lane;
                body.StartTime = StartTime;
                body.SlideInfo = SlideInfo;
                body.Samples = NodeSamples.Any() ? NodeSamples.Last() : new List<HitSampleInfo>();
            }
        }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
        public override Judgement CreateJudgement() => new IgnoreJudgement();
    }
}
