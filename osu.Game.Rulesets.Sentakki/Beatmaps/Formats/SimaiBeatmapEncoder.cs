using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using static System.FormattableString;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Formats;

/// <summary>
/// Basic simai encoder that perfectly encodes a sentakki chart by using the beatLength notation of simai, encoding the intervals between notes in absolute time.
///
/// This is officially supported by Celeca's simai spec, and is supported by AstroDX/SimaiSharp, but not by Majdata[play/edit]
/// </summary>
public class SimaiBeatmapEncoder
{
    private static readonly Dictionary<Vector2, string> touch_position_mapping
        = SentakkiPlayfield
          .LANEANGLES.SelectMany<float, KeyValuePair<Vector2, string>>((angle, index) =>
              [
                  new KeyValuePair<Vector2, string>(
                      MathExtensions.PointOnCircle(130, angle),
                      $"B{index + 1}"
                  ),
                  new KeyValuePair<Vector2, string>(
                      MathExtensions.PointOnCircle(190, angle - 22.5f),
                      $"E{index + 1}"
                  ),
                  new KeyValuePair<Vector2, string>(
                      MathExtensions.PointOnCircle(270, angle),
                      $"A{index + 1}"
                  ),
                  new KeyValuePair<Vector2, string>(
                      MathExtensions.PointOnCircle(270, angle - 22.5f),
                      $"D{index + 1}"
                  ),
              ]
          )
          .Append(new KeyValuePair<Vector2, string>(Vector2.Zero, "C"))
          .ToDictionary();

    protected IBeatmap<SentakkiHitObject> Beatmap;

    public SimaiBeatmapEncoder(IBeatmap<SentakkiHitObject> beatmap)
    {
        Beatmap = beatmap;
    }

    public void Encode(TextWriter writer)
    {
        writer.WriteLine(Invariant($"&comment1=Sentakki flavoured simai v0"));
        writer.WriteLine(Invariant($"&comment2=Backwards compatible with standard simai format used by Majdata / Astro"));

        handleMetadata(writer);
        handleBeatmap(writer);
    }

    private void handleMetadata(TextWriter writer)
    {
        var metadata = Beatmap.Metadata;

        // No unicode title, title will be forced to romanised title (which is guaranteed to exist)
        if (string.IsNullOrEmpty(metadata.TitleUnicode))
        {
            writer.WriteLine(Invariant($"&title={metadata.Title}"));
        }
        else
        {
            writer.WriteLine(Invariant($"&title={metadata.TitleUnicode}"));
            writer.WriteLine(Invariant($"&titleRomanised={metadata.Title}"));
        }

        // No unicode artist, artist will be forced to romanised artist (which is guaranteed to exist)
        if (string.IsNullOrEmpty(metadata.ArtistUnicode))
        {
            writer.WriteLine(Invariant($"&artist={metadata.Artist}"));
        }
        else
        {
            writer.WriteLine(Invariant($"&artist={metadata.ArtistUnicode}"));
            writer.WriteLine(Invariant($"&artistRomanised={metadata.Artist}"));
        }

        if (!string.IsNullOrEmpty(metadata.Source))
            writer.WriteLine(Invariant($"&source={metadata.Source}"));

        // We repeat the author field, the description is used by simai to include additional info. Chart author is one of them.
        writer.WriteLine(Invariant($"&author={metadata.Author.Username}"));
        writer.WriteLine(Invariant($"&des={metadata.Author.Username}"));

        // Encode tags in-case someone decides to import  a simai chart encoded by sentakki
        if (!string.IsNullOrEmpty(Beatmap.Metadata.Tags)) writer.WriteLine(Invariant($"&tags={Beatmap.Metadata.Tags}"));

        writer.WriteLine(Invariant($"&wholebpm={(int)double.Round(Beatmap.BeatmapInfo.BPM)}"));

        // This is astroDX specific I believe
        writer.WriteLine(Invariant($"&demoseek={metadata.PreviewTime / 1000}"));
        writer.WriteLine(Invariant($"&demolen={10}"));
    }

    private void handleBeatmap(TextWriter writer)
    {
        writer.WriteLine($"&lv_7=洗");
        writer.WriteLine($"&inote_7={CreateSimaiChart()}");
    }

    protected virtual string CreateSimaiChart()
    {
        var hitObjectsGroups = Beatmap.HitObjects.GroupBy(h => h.StartTime).OrderBy(g => g.Key).Select(g => new TimedBeatmapEvent
        {
            Time = g.Key,
            BeatmapEvent = new HitObjectGroup([.. g])
        });

        var timingPoints = Beatmap.ControlPointInfo.TimingPoints.Select(t => new TimedBeatmapEvent
        {
            Time = t.Time,
            BeatmapEvent = new TimingPointMarker(t)
        });

        TimedBeatmapEvent[] events = [.. hitObjectsGroups, .. timingPoints];

        if (events.Length == 0)
            return ",E";

        StringBuilder maidataBuilder = new StringBuilder();

        // Add padding timingPoint prior to first event
        if (events[0].Time > 0)
            maidataBuilder.Append(Invariant($"\n({((TimingPointMarker)timingPoints.First().BeatmapEvent).timingPoint.BPM}){{#{events[0].Time / 1000:F3}}},"));

        for (int i = 0; i < events.Length; ++i)
        {
            var group = events[i];

            double nextTime = group.Time;

            if (group.BeatmapEvent is TimingPointMarker tp)
                maidataBuilder.Append(Invariant($"({tp.timingPoint.BPM})"));



            if (i < events.Length - 1)
            {
                var nextGroup = events[i + 1];
                double delta = nextGroup.Time - group.Time;

                maidataBuilder.Append(Invariant($"\n{{#{delta / 1000:F3}}}"));
            }

            if (group.BeatmapEvent is HitObjectGroup hog)
            {
                var hitobjects = hog.HitObjects;

                for (int j = 0; j < hitobjects.Length; ++j)
                {
                    var hitobject = hitobjects[j];

                    string hitObjectString = hitobject switch
                    {
                        Tap t => TapToString(t),
                        Hold h => HoldToString(h),
                        Slide s => SlideToString(s),
                        Touch tc => TouchToString(tc),
                        TouchHold th => TouchHoldToString(th),
                        _ => ""
                    };

                    maidataBuilder.Append(hitObjectString);
                    if (j < hitobjects.Length - 1)
                        maidataBuilder.Append('/');
                }
            }

            maidataBuilder.Append(',');
        }

        maidataBuilder.Append(",\nE");

        return maidataBuilder.ToString();
    }

    protected static string TapToString(Tap tap) => $"{tap.Lane + 1}{(tap.Break ? "b" : "")}{(tap.Ex ? "x" : "")}";
    protected static string HoldToString(Hold hold) => Invariant($"{hold.Lane + 1}h{(hold.Break ? "b" : "")}{(hold.Ex ? "x" : "")}[#{hold.Duration / 1000:F3}]");

    protected string SlideToString(Slide slide)
    {
        StringBuilder slideBuilder = new();

        slideBuilder.Append($"{slide.Lane + 1}");

        // Tap has break
        if (slide.Break)
            slideBuilder.Append('b');

        // Star appearances
        if (slide.TapType == Slide.TapTypeEnum.Star && slide.SlideInfoList.Count == 0)
            slideBuilder.Append("$$");
        else if (slide.TapType == Slide.TapTypeEnum.Tap)
            slideBuilder.Append('@');
        else if (slide.TapType == Slide.TapTypeEnum.None)
            slideBuilder.Append('?');

        // Tap EX
        if (slide.Ex)
            slideBuilder.Append('x');

        if (slide.SlideInfoList.Count > 0)
        {
            for (int i = 0; i < slide.SlideInfoList.Count; ++i)
            {
                var slideInfo = slide.SlideInfoList[i];
                int currentLane = slide.Lane;

                foreach (var part in slideInfo.SlidePathParts)
                {
                    int endLane = (currentLane + part.EndOffset).NormalizeLane();
                    slideBuilder.Append(shapeForSlidePart(currentLane, part, slideInfo.SlidePathParts.Length > 1));
                    slideBuilder.Append(endLane + 1);
                    currentLane = endLane;
                }

                double durationWithoutDelay = slideInfo.Duration - slideInfo.ShootDelay;

                slideBuilder.Append(Invariant($"[{slideInfo.ShootDelay / 1000:F3}##{durationWithoutDelay / 1000:F3}]"));

                if (slideInfo.Break)
                    slideBuilder.Append('b');

                if (i < slide.SlideInfoList.Count - 1)
                    slideBuilder.Append('*');
            }
        }

        return slideBuilder.ToString();
    }

    private static string shapeForSlidePart(int startLane, in SlideBodyPart part, bool inChain)
    {
        switch (part.Shape)
        {
            case SlidePaths.PathShapes.Straight:
                return "-";

            case SlidePaths.PathShapes.Circle:
                bool startsFromBottom = ((startLane + 2) % 8) >= 4;
                bool facingLeft = part.Mirrored ^ startsFromBottom;

                return facingLeft ? "<" : ">";

            case SlidePaths.PathShapes.V:
                // Majdata(view/play) shits itself because it is not robust enough to handle slide equivalences
                if (part.EndOffset == 4)
                    return "-";

                return "v";

            case SlidePaths.PathShapes.U:
                return part.Mirrored ? "q" : "p";

            case SlidePaths.PathShapes.Thunder:
                return part.Mirrored ? "z" : "s";

            case SlidePaths.PathShapes.Cup:
                return part.Mirrored ? "qq" : "pp";

            case SlidePaths.PathShapes.Fan:
                // Slide chain into a fan is only supported in sentakki
                // Coerce the slide into a straight
                if (inChain)
                    return "-";

                return "w";

            default:
                return "-";
        }
    }

    protected static string TouchToString(Touch touch) => $"{positionMappingFor(touch.Position)}{(touch.Break ? "b" : "")}{(touch.Ex ? "x" : "")}";

    private static string positionMappingFor(Vector2 position) => touch_position_mapping.MinBy(kv => Vector2.DistanceSquared(position, kv.Key)).Value;

    protected static string TouchHoldToString(TouchHold touchHold) =>
        Invariant($"{positionMappingFor(touchHold.Position)}h{(touchHold.Break ? "b" : "")}[#{touchHold.Duration / 1000:F3}]");

    public void SerializeToFile()
    {
        var metadata = Beatmap.BeatmapInfo.Metadata;
        string path = $"(sen) {metadata.ArtistUnicode} - {metadata.TitleUnicode} ({Beatmap.BeatmapInfo.DifficultyName}).txt";
        var file = File.CreateText(path);
        Encode(file);
        file.Close();
    }

    public class TimedBeatmapEvent
    {
        public double Time;
        public IBeatmapEvent BeatmapEvent = null!;
    }

    public interface IBeatmapEvent;
    public record class HitObjectGroup(SentakkiHitObject[] HitObjects) : IBeatmapEvent { }

    public record class TimingPointMarker(TimingControlPoint timingPoint) : IBeatmapEvent { };
}
