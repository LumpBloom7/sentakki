using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects;

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

    /// <summary>
    /// The duration of this slide, determined by the longest slide body in the list.
    /// </summary>
    /// <remarks>
    /// If there are no slide bodies, this will return a duration of 0.
    /// <br/>
    /// When increasing/decreasing the duration, each slide body's duration will be scaled proportionally.
    /// </remarks>
    public double Duration
    {
        get => SlideInfoList.Select(static t => t.Duration).Prepend(0).Max();
        set
        {
            double max = SlideInfoList.Select(static t => t.Duration).Prepend(0).Max();

            if (max == value)
                return;

            foreach (var s in SlideInfoList)
            {
                double ratio = s.Duration / max;

                s.Duration = ratio * value;
            }
        }
    }

    public TapTypeEnum TapType = TapTypeEnum.Star;

    public double EndTime => StartTime + Duration;

    public override Color4 DefaultNoteColour => Color4.Aqua;
    public List<SlideBodyInfo> SlideInfoList = [];

    [JsonIgnore]
    public Tap SlideTap { get; private set; } = null!;

    [JsonIgnore]
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
        SlideBodies = [];

        foreach (var slideInfo in SlideInfoList)
        {
            SlideBody body;
            AddNested(body = new SlideBody(slideInfo)
            {
                Lane = slideInfo.RelativeEndLane + Lane,
                StartTime = StartTime,
                Samples = Samples,
            });

            SlideBodies.Add(body);
        }
    }

    protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    public override Judgement CreateJudgement() => new IgnoreJudgement();
}
