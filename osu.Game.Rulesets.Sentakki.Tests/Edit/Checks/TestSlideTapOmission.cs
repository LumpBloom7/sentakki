using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Edit.Checks;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Sentakki.Tests.Edit.Checks;

[TestFixture]
public class TestSlideTapOmission
{
    private CheckSlideTapOmission check = null!;

    [SetUp]
    public void Setup()
    {
        check = new CheckSlideTapOmission();
    }

    [Test]
    public void NoOmittedSlideTaps()
    {
        var hitobjects = Enumerable.Range(0, 1000).Select(_ => new Slide
        {
            TapType = Random.Shared.Next(0, 2) == 0 ? Slide.TapTypeEnum.Star : Slide.TapTypeEnum.Tap
        }).Cast<HitObject>().ToList();

        var issues = check.Run(getContext(hitobjects));

        Assert.That(!issues.Any(), $"No issues should be reported if all {nameof(Slide)}s' tap type is {nameof(Slide.TapTypeEnum.Star)} or {nameof(Slide.TapTypeEnum.Tap)}.");
    }

    [Test]
    public void WithOmittedSlideTaps()
    {
        var hitobjects = Enumerable.Range(0, 1000).Select(_ => new Slide
        {
            TapType = (Slide.TapTypeEnum)Random.Shared.Next(0, 3),
        }).Cast<HitObject>().ToList();

        var slidesWithOmittedTaps = hitobjects.OfType<Slide>().Where(s => s.TapType is Slide.TapTypeEnum.None).ToHashSet();

        var issues = check.Run(getContext(hitobjects)).ToList();

        Assert.That(issues.Count == slidesWithOmittedTaps.Count, "There should be 1 issue for every slide with an ommitted tap.");

        Assert.That(issues.All(i => i.Template.Type == Rulesets.Edit.Checks.Components.IssueType.Warning));
        Assert.That(issues.All(i => i.Template is CheckSlideTapOmission.OmittedSlideTapIssueTemplate));
        Assert.That(issues.SelectMany(i => i.HitObjects).ToHashSet().SetEquals(slidesWithOmittedTaps), "Issues do not hold the same slide objects.");
    }

    private BeatmapVerifierContext getContext(List<HitObject> hitObjects)
    {
        var beatmap = new Beatmap()
        {
            HitObjects = hitObjects
        };

        return new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
    }
}
