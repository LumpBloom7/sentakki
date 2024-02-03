using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Bindables;
using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Slide : SentakkiLanedHitObject, IHasDuration
    {
        public enum TapTypeEnum
        {
            Star,
            Tap,
            None,
        }

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
                    // Lets remove samples from slide completions
                    //Samples = NodeSamples.Any() ? NodeSamples.Last() : new List<HitSampleInfo>()
                });

                SlideBodies.Add(body);
            }
        }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
        public override Judgement CreateJudgement() => new IgnoreJudgement();
    }
}
