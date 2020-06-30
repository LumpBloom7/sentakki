using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osu.Game.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Statistics;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Graphics;
using osu.Game.Tests.Visual;
using System;
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
            new HitEvent(0,HitResult.Perfect,new Hold(),new Tap(), null),
        };
        public TestSceneJudgementChart()
        {
            Add(new JudgementChart(testevents));
        }
    }
}
