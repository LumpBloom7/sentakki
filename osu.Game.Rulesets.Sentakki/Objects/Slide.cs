using osu.Framework.Bindables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using System;
using System.Collections.Generic;
using System.Threading;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Slide : SentakkiLanedHitObject
    {
        public override Color4 NoteColor => IsBreak ? Color4.OrangeRed : HasTwin ? Color4.Gold : Color4.Aqua;

        public List<SentakkiSlideInfo> SlideInfoList = new List<SentakkiSlideInfo>();

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            AddNested(new Tap
            {
                LaneBindable = { BindTarget = LaneBindable },
                StartTime = StartTime,
                Samples = Samples,
                IsBreak = IsBreak
            });
            createSlideBodies();
        }

        private void createSlideBodies()
        {
            foreach (var SlideInfo in SlideInfoList)
            {
                AddNested(new SlideBody
                {
                    Lane = SlideInfo.SlidePath.EndLane + Lane,
                    StartTime = StartTime,
                    SlideInfo = SlideInfo
                });
            }
        }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
        public override Judgement CreateJudgement() => new IgnoreJudgement();
    }
}
