using osu.Framework.Bindables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using System;
using System.Collections.Generic;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Slide : SentakkiHitObject, IHasDuration
    {
        public override Color4 NoteColor => IsBreak ? Color4.OrangeRed : HasTwin ? Color4.Gold : Color4.Aqua;

        public static readonly float SLIDE_CHEVRON_DISTANCE = 25;
        public List<int> SlidePathIDs;

        // The delay (in beats) before the animation star starts moving along the path
        private readonly BindableInt slideShootDelay = new BindableInt(1);

        public int SlideShootDelay
        {
            get => slideShootDelay.Value;
            set => slideShootDelay.Value = value;
        }

        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }
        public double Duration { get; set; }

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();

            AddNested(new Tap { LaneBindable = { BindTarget = LaneBindable }, StartTime = StartTime, Samples = Samples, IsBreak = IsBreak });
            createSlideBodies();
        }

        private void createSlideBodies()
        {
            foreach (var SlideID in SlidePathIDs)
            {
                AddNested(new SlideBody
                {
                    Lane = SlidePaths.VALIDPATHS[SlideID].EndLane + Lane,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    SlideShootDelay = SlideShootDelay,
                    SlidePath = SlidePaths.VALIDPATHS[SlideID]
                });
            }
        }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
        public override Judgement CreateJudgement() => new IgnoreJudgement();
    }
}
