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
public class TestMismatchedSlideTaps
{
    private CheckMismatchedSlideTaps check = null!;

    [SetUp]
    public void Setup()
    {
        check = new CheckMismatchedSlideTaps();
    }

    [Test]
    public void TestMatchingSlideTapsConcurrent()
    {
        {
            List<HitObject> hitobjects = [
                    new Slide {
                        Lane = 0,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.Star,
                    },
                    new Slide {
                        Lane = 0,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.Star,
                    },
                    new Slide {
                        Lane = 0,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.Star,
                    }
                ];

            var issues = check.Run(getContext(hitobjects));

            Assert.That(!issues.Any(), "Concurrent slides with star should not have an issue.");
        }

        {
            List<HitObject> hitobjects = [
                    new Slide {
                        Lane = 0,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.Tap,
                    },
                    new Slide {
                        Lane = 0,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.Tap,
                    },
                    new Slide {
                        Lane = 0,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.Tap,
                    }
                ];

            var issues = check.Run(getContext(hitobjects));

            Assert.That(!issues.Any(), "Concurrent slides with tap should not have an issue.");
        }

        {
            List<HitObject> hitobjects = [
                    new Slide {
                        Lane = 0,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.None,
                    },
                    new Slide {
                        Lane = 0,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.None,
                    },
                    new Slide {
                        Lane = 0,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.None,
                    }
                ];

            var issues = check.Run(getContext(hitobjects));

            Assert.That(!issues.Any(), "Concurrent slides with no taps should not have an issue.");
        }
    }

    [Test]
    public void TestNonMatchingSlideTapsConcurrent()
    {
        List<List<HitObject>> hitobjectGroups = [
                [
                    new Slide {
                        Lane = 0,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.Star,
                    },
                    new Slide {
                        Lane = 0,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.Tap,
                    },
                    new Slide {
                        Lane = 0,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.None,
                    }
                ],

                [
                    new Slide {
                        Lane = 1,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.Tap,
                    },
                    new Slide {
                        Lane = 1,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.Tap,
                    },
                    new Slide {
                        Lane = 1,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.None,
                    }
                ],

                [
                    new Slide {
                        Lane = 1,
                        StartTime = 25,
                        TapType = Slide.TapTypeEnum.Tap,
                    },
                    new Slide {
                        Lane = 1,
                        StartTime = 25,
                        TapType = Slide.TapTypeEnum.Tap,
                    },
                    new Slide {
                        Lane = 1,
                        StartTime = 25,
                        TapType = Slide.TapTypeEnum.None,
                    }
                ]
            ];

        var issues = check.Run(getContext([.. hitobjectGroups.SelectMany(s => s)])).ToArray();

        Assert.That(issues.Length == 3, "concurrent slide taps with mismatched tap types should only generate 1 issue for each group");

        for (int i = 0; i < issues.Length; ++i)
            Assert.That(issues[i].HitObjects.ToHashSet().SetEquals(hitobjectGroups[i].ToHashSet()), "All slides in a group should be included in the issue");

        Assert.That((Slide.TapTypeEnum)issues[0].Arguments[0] == Slide.TapTypeEnum.Star, "When any slide has a slide tap, that is preferred.");
        Assert.That((Slide.TapTypeEnum)issues[1].Arguments[0] == Slide.TapTypeEnum.Tap, "Lacking a slide tap, if any have a regular tap, that is preferred.");
        Assert.That((Slide.TapTypeEnum)issues[1].Arguments[0] == Slide.TapTypeEnum.Tap, "Lacking a slide tap, if any have a regular tap, that is preferred.");
    }

    [Test]
    public void TestNonMatchingSlideTapsNotConcurrent()
    {
        List<List<HitObject>> hitobjectGroups = [
                [ // Not concurrent, same lane
                    new Slide {
                        Lane = 0,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.Star,
                    },
                    new Slide {
                        Lane = 0,
                        StartTime = 25,
                        TapType = Slide.TapTypeEnum.Tap,
                    },
                    new Slide {
                        Lane = 0,
                        StartTime = 35,
                        TapType = Slide.TapTypeEnum.None,
                    }
                ],

                [ // Concurrent, different lane
                    new Slide {
                        Lane = 5,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.Tap,
                    },
                    new Slide {
                        Lane = 7,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.Tap,
                    },
                    new Slide {
                        Lane = 8,
                        StartTime = 0,
                        TapType = Slide.TapTypeEnum.None,
                    }
                ],
            ];

        var issues = check.Run(getContext([.. hitobjectGroups.SelectMany(s => s)]));

        Assert.That(!issues.Any(), "Non concurrent slide taps should never generate issues.");
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
