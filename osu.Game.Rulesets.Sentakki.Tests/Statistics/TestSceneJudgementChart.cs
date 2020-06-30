using NUnit.Framework;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Statistics;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Tests.Visual;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Sentakki.Tests.Statistics
{
    [TestFixture]
    public class TestSceneJudgementChart : OsuTestScene
    {
        private List<HitEvent> testevents = new List<HitEvent>
        {
            new HitEvent(0,HitResult.Perfect,new Tap(),new Tap(), null),
            new HitEvent(0,HitResult.Perfect,new Tap(),new Tap(), null),
            new HitEvent(0,HitResult.Perfect,new Tap(),new Tap(), null),
            new HitEvent(0,HitResult.Perfect,new Tap(),new Tap(), null),
            new HitEvent(0,HitResult.Perfect,new Tap(),new Tap(), null),
            new HitEvent(0,HitResult.Perfect,new Tap(),new Tap(), null),
            new HitEvent(0,HitResult.Good,new Tap(),new Tap(), null),
            new HitEvent(0,HitResult.Good,new Tap(),new Tap(), null),
            new HitEvent(0,HitResult.Good,new Tap(),new Tap(), null),
            new HitEvent(0,HitResult.Good,new Tap(),new Tap(), null),
            new HitEvent(0,HitResult.Good,new Tap(),new Tap(), null),
            new HitEvent(0,HitResult.Meh,new Tap(),new Tap(), null),
            new HitEvent(0,HitResult.Meh,new Tap(),new Tap(), null),
            new HitEvent(0,HitResult.Meh,new Tap(),new Tap(), null),
            new HitEvent(0,HitResult.Miss,new Tap(),new Tap(), null),
            new HitEvent(0,HitResult.Miss,new Tap(),new Tap(), null),
            new HitEvent(0,HitResult.Perfect,new Hold.HoldHead(),new Tap(), null),
            new HitEvent(0,HitResult.Perfect,new Hold.HoldTail(),new Tap(), null),
        };
        public TestSceneJudgementChart()
        {
            Add(new JudgementChart(testevents));
        }
    }
}
