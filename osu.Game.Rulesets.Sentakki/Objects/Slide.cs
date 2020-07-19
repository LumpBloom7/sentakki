using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Objects;
using System.Collections.Generic;
using System.Linq;
using System;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Slide : SentakkiHitObject, IHasDuration
    {
        public static readonly float SLIDE_CHEVRON_DISTANCE = 25;
        public SliderPath SlidePath;

        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }
        public double Duration { get; set; }

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();

            var distance = SlidePath.Distance;
            int chevrons = (int)Math.Ceiling(distance / Slide.SLIDE_CHEVRON_DISTANCE);
            double nodeInterval = 1.0 / chevrons * 5; // Node every 5 chevrons.

            for (double progress = nodeInterval; progress < 1; progress += nodeInterval)
            {
                if (progress + nodeInterval > 1)
                    progress = 1;
                AddNested(new SlideNode { Progress = progress });
            }
        }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        public class SlideNode : SentakkiHitObject
        {
            public double Progress { get; set; }
            protected override HitWindows CreateHitWindows() => HitWindows.Empty;
            public override Judgement CreateJudgement() => new IgnoreJudgement();
        }
    }
}
