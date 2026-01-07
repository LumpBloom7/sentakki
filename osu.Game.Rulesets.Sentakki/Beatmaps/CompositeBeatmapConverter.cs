using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Beatmaps.Converter;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Beatmaps;

public class CompositeBeatmapConverter : BeatmapConverter<SentakkiHitObject>
{
    public override bool CanConvert() => Beatmap.HitObjects.All(h => h is IHasPosition) || Beatmap.BeatmapInfo.Ruleset.ShortName == "Sentakki";

    public ConversionFlags Flags;

    private readonly Ruleset ruleset;

    public CompositeBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
        : base(beatmap, ruleset)
    {
        this.ruleset = ruleset;
    }

    protected override Beatmap<SentakkiHitObject> ConvertBeatmap(IBeatmap original, CancellationToken cancellationToken)
    {
        if (Beatmap.HitObjects.All(h => h is SentakkiHitObject))
        {
            return cloneBeatmap(original);
        }

        BeatmapConverter<SentakkiHitObject> converter;

        if (Flags.HasFlag(ConversionFlags.OldConverter))
            converter = new SentakkiBeatmapConverterOld(original, ruleset) { ConversionFlags = Flags };
        else
            converter = new SentakkiBeatmapConverter(original, ruleset) { ConversionFlags = Flags };

        return (SentakkiBeatmap)converter.Convert(cancellationToken);
    }

    private Beatmap<SentakkiHitObject> cloneBeatmap(IBeatmap original)
    {
        var beatmap = CreateBeatmap();
        beatmap.BeatmapInfo = original.BeatmapInfo;
        beatmap.ControlPointInfo = original.ControlPointInfo;
        beatmap.Breaks = original.Breaks;
        beatmap.UnhandledEventLines = original.UnhandledEventLines;
        beatmap.HitObjects = cloneHitObjects(original.HitObjects);

        // For the 0 people that insist on using it.
        beatmap.Bookmarks = original.Bookmarks;
        beatmap.TimelineZoom = original.TimelineZoom;
        return beatmap;
    }

    private List<SentakkiHitObject> cloneHitObjects(IReadOnlyList<HitObject> hitObjects)
    {
        var list = new List<SentakkiHitObject>(hitObjects.Count);

        foreach (HitObject ho in hitObjects)
        {
            switch (ho)
            {
                case Tap tap:
                    list.Add(new Tap
                    {
                        Lane = tap.Lane,
                        StartTime = tap.StartTime,
                        Break = tap.Break,
                        Ex = tap.Ex,
                        Samples = tap.Samples
                    });
                    break;

                case Hold hold:
                    list.Add(new Hold
                    {
                        Lane = hold.Lane,
                        StartTime = hold.StartTime,
                        Duration = hold.Duration,
                        Break = hold.Break,
                        Ex = hold.Ex,
                        Samples = hold.Samples
                    });
                    break;

                case Slide slide:
                    var slideInfoList = new List<SlideBodyInfo>();
                    foreach (var slideInfo in slide.SlideInfoList)
                    {
                        var slidePathParts = new SlideBodyPart[slideInfo.SlidePathParts.Length];

                        for (int i = 0; i < slideInfo.SlidePathParts.Length; ++i)
                        {
                            ref var part = ref slideInfo.SlidePathParts[i];
                            slidePathParts[i] = new SlideBodyPart(part.Shape, part.EndOffset, part.Mirrored);
                        }
                        slideInfoList.Add(new SlideBodyInfo
                        {
                            Duration = slideInfo.Duration,
                            Break = slideInfo.Break,
                            ShootDelay = slideInfo.ShootDelay,
                            SlidePathParts = slidePathParts
                        });
                    }

                    list.Add(new Slide
                    {
                        Samples = slide.Samples,
                        Lane = slide.Lane,
                        Break = slide.Break,
                        Ex = slide.Ex,
                        SlideInfoList = slideInfoList,
                        StartTime = slide.StartTime,
                        TapType = slide.TapType
                    });
                    break;

                case TouchHold th:
                    list.Add(new TouchHold
                    {
                        StartTime = th.StartTime,
                        Samples = th.Samples,
                        Duration = th.Duration,
                        Position = th.Position,
                        Break = th.Break,
                    });
                    break;

                case Touch touch:
                    list.Add(new Touch
                    {
                        Position = touch.Position,
                        Samples = touch.Samples,
                        StartTime = touch.StartTime,
                        Ex = touch.Ex,
                        Break = touch.Break,
                    });
                    break;

            }
        }

        return list;
    }

    protected override Beatmap<SentakkiHitObject> CreateBeatmap() => new SentakkiBeatmap();
}
