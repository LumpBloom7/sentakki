using System;
using System.Collections.Generic;
using System.Threading;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Slide : SentakkiLanedHitObject, IHasDuration
    {
        public override double MaximumJudgementOffset
        {
            get
            {
                double offset = 0.0;
                foreach (var nested in NestedHitObjects)
                    offset = Math.Max(offset, nested.MaximumJudgementOffset);

                return offset;
            }
        }

        public enum TapTypeEnum
        {
            Star,
            Tap,
            None,
        }

        protected override bool PlaysBreakSample => false;

        public double Duration
        {
            get
            {
                double max = 0;
                for (int i = 0; i < SlideInfoList.Count; ++i)
                    max = Math.Max(max, SlideInfoList[i].Duration);

                return max;
            }
            set
            {
                if (Duration == 0)
                {
                    foreach (var slide in SlideInfoList)
                        slide.Duration = value;

                    return;
                }

                double ratio = value / Duration;

                foreach (var slide in SlideInfoList)
                    slide.Duration *= ratio;
            }
        }

        public TapTypeEnum TapType = TapTypeEnum.Star;

        public double EndTime => StartTime + Duration;

        public override Color4 DefaultNoteColour => Color4.Aqua;
        public List<SlideBodyInfo> SlideInfoList = new List<SlideBodyInfo>();

        public Tap SlideTap { get; private set; } = null!;

        public IList<SlideBody> SlideBodies { get; private set; } = null!;

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            if (TapType is not TapTypeEnum.None)
            {
                Tap tap = SlideTap = TapType is TapTypeEnum.Tap ? new Tap() : new SlideTap();
                tap.LaneBindable.BindTarget = LaneBindable;
                tap.StartTime = StartTime;
                tap.Samples = Samples;
                tap.Break = Break;
                tap.Ex = Ex;

                AddNested(tap);
            }

            createSlideBodies();
        }

        private void createSlideBodies()
        {
            SlideBodies = new List<SlideBody>();

            foreach (var slideInfo in SlideInfoList)
            {
                SlideBody body;
                AddNested(body = new SlideBody(slideInfo)
                {
                    Lane = slideInfo.SlidePath.EndLane + Lane,
                    StartTime = StartTime,
                    Samples = Samples,
                });

                SlideBodies.Add(body);
            }
        }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
        public override Judgement CreateJudgement() => new IgnoreJudgement();
    }
}
