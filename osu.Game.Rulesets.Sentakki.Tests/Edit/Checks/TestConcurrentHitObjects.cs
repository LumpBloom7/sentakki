using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Edit.Checks;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Tests.Edit.Checks;

[TestFixture]
public class TestConcurrentHitObject
{
    private CheckConcurrentLaneHitObjects check = null!;

    [SetUp]
    public void Setup()
    {
        check = new CheckConcurrentLaneHitObjects();
    }

    [Test]
    public void SeparateTaps()
    {
        var issues = check.Run(getContext([
            new Tap{
                StartTime = 0,
            },
            new Tap{
                StartTime = 500,
            }
        ]));

        Assert.That(issues, Is.Empty);
    }

    [Test]
    public void DoubleTapsDifferentLane()
    {
        var issues = check.Run(getContext([
            new Tap{
                Lane = 0,
                StartTime = 0,
            },
            new Tap{
                Lane = 1,
                StartTime = 0,
            }
        ]));

        Assert.That(issues, Is.Empty);
    }

    [Test]
    public void TripleTapsDifferentLane()
    {
        var issues = check.Run(getContext([
            new Tap{
                Lane = 0,
                StartTime = 0,
            },
            new Tap{
                Lane = 1,
                StartTime = 0,
            },
            new Tap{
                Lane = 2,
                StartTime = 0,
            },
        ]));

        Assert.That(issues.Count() == 1);
    }

    [Test]
    public void ConcurrentTaps()
    {
        var issues = check.Run(getContext([
            new Tap{
                Lane = 0,
                StartTime = 0,
            },
            new Tap{
                StartTime = 0,
            }
        ]));

        Assert.That(issues.Count() == 1);
    }


    [Test]
    public void BasicallyConcurrentTaps()
    {
        var issues = check.Run(getContext([
            new Tap{
                Lane = 0,
                StartTime = 0,
            },
            new Tap{
                StartTime = 1,
            }
        ]));

        Assert.That(issues.Count() == 1);
    }

    [Test]
    public void AlmostConcurrentTaps()
    {
        var issues = check.Run(getContext([
            new Tap{
                Lane = 0,
                StartTime = 0,
            },
            new Tap{
                StartTime = 8,
            }
        ]));

        Assert.That(issues.Count() == 1);
    }

    [Test]
    public void TapConcurrentWithHold()
    {
        var issues = check.Run(getContext([
            new Tap{
                Lane = 0,
                StartTime = 10,
            },
            new Hold{
                StartTime = 0,
                Duration = 50
            }
        ]));

        Assert.That(issues.Count() == 1);
    }

    [Test]
    public void TapDuringHoldDifferentLanes()
    {
        var issues = check.Run(getContext([
            new Tap{
                Lane = 1,
                StartTime = 10,
            },
            new Hold{
                StartTime = 0,
                Duration = 50
            }
        ]));

        Assert.That(issues, Is.Empty);
    }

    [Test]
    public void DoubleTapDuringHoldDifferentLanes()
    {
        var issues = check.Run(getContext([
            new Tap{
                Lane = 1,
                StartTime = 10,
            },

            new Tap{
                Lane = 2,
                StartTime = 10,
            },

            new Hold{
                StartTime = 0,
                Duration = 50
            }
        ]));

        Assert.That(issues.Count() == 1);
    }

    [Test]
    public void TapConcurrentWithSlideWithoutTap()
    {
        var issues = check.Run(getContext([
            new Tap{
                Lane = 0,
                StartTime = 10,
            },
            new Slide{
                StartTime = 0,
                TapType = Slide.TapTypeEnum.None,
                SlideInfoList = [
                    new SlideBodyInfo
                    {
                        Duration = 200
                    }
                ]
            }
        ]));

        Assert.That(issues.Count() == 1);
    }

    [Test]
    public void TapConcurrentWithSlideTap()
    {
        var issues = check.Run(getContext([
            new Tap{
                Lane = 0,
                StartTime = 10,
            },
            new Slide{
                StartTime = 0,
                SlideInfoList = [
                    new SlideBodyInfo
                    {
                        Duration = 200
                    }
                ]
            }
        ]));

        Assert.That(issues.Count() == 1);
    }

    [Test]
    public void DoubleTapDuringSlideWait()
    {
        var issues = check.Run(getContext([
            new Tap{
                Lane = 1,
                StartTime = 10,
            },
            new Tap{
                Lane = 2,
                StartTime = 10,
            },
            new Slide{
                StartTime = 0,
                SlideInfoList = [
                    new SlideBodyInfo
                    {
                        Duration = 200
                    }
                ]
            }
        ]));

        Assert.That(issues, Is.Empty);
    }

    [Test]
    public void TripleTapDuringSlideWait()
    {
        var issues = check.Run(getContext([
            new Tap{
                Lane = 1,
                StartTime = 10,
            },
            new Tap{
                Lane = 2,
                StartTime = 10,
            },
            new Slide{
                StartTime = 0,
                SlideInfoList = [
                    new SlideBodyInfo
                    {
                        Duration = 200
                    }
                ]
            }
        ]));

        Assert.That(issues.Count() == 1);
    }

    [Test]
    public void DoubleTapDuringSlideMove()
    {
        var issues = check.Run(getContext([
            new Tap{
                Lane = 1,
                StartTime = 10,
            },
            new Tap{
                Lane = 2,
                StartTime = 10,
            },
            new Slide{
                StartTime = 0,
                SlideInfoList = [
                    new SlideBodyInfo
                    {
                        Duration = 200
                    }
                ]
            }
        ]));

        Assert.That(issues.Count() == 1);
    }

    [Test]
    public void SlideEndPerfectWindowWithinTapPerfectWindow()
    {
        var issues = check.Run(getContext([
            new Tap
            {
                Lane = 4,
                StartTime = 16,
            },
            new Slide{
                StartTime = 0,
                SlideInfoList = [
                    new SlideBodyInfo
                    {
                        Segments = [new SlideSegment(PathShape.Straight, 4, false)],
                        Duration = 16
                    }
                ]
            }
       ]));

        Assert.That(issues, Is.Empty);
    }

    [Test]
    public void SlideEndPerfectWindowOverlapsTapSubPerfectWindow()
    {
        var issues = check.Run(getContext([
            new Tap
            {
                Lane = 4,
                StartTime = 5000,
            },
            new Slide{
                StartTime = 0,
                SlideInfoList = [
                    new SlideBodyInfo
                    {
                        Segments = [new SlideSegment(PathShape.Straight, 4, false)],
                        Duration = 5000
                    }
                ]
            }
       ]));

        Assert.That(issues.Count() == 1);
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
