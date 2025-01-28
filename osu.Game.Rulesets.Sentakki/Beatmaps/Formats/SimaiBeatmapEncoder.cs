using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using static System.FormattableString;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Formats;

public class SimaiBeatmapEncoder
{
    private static Dictionary<Vector2, string> TouchPositionMapping = SentakkiPlayfield
        .LANEANGLES.SelectMany<float, KeyValuePair<Vector2, string>>(
            (angle, index) =>

                [
                    new(
                        SentakkiExtensions.GetCircularPosition(130, angle),
                        $"B{index+1}"
                    ),
                    new(
                        SentakkiExtensions.GetCircularPosition(190, angle - 22.5f),
                        $"E{index+1}"
                    ),
                    new(
                        SentakkiExtensions.GetCircularPosition(270, angle),
                        $"A{index+1}"
                    ),
                    new(
                        SentakkiExtensions.GetCircularPosition(270, angle - 22.5f),
                        $"D{index+1}"
                    ),
                ]
        )
        .Append(new(new(0, 0), "C"))
        .ToDictionary();

    protected IBeatmap<SentakkiHitObject> beatmap;

    public SimaiBeatmapEncoder(IBeatmap<SentakkiHitObject> beatmap)
    {
        this.beatmap = beatmap;
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
        var metadata = beatmap.Metadata;
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
            writer.WriteLine(Invariant($"&artist={metadata.TitleUnicode}"));
            writer.WriteLine(Invariant($"&artistRomanised={metadata.Title}"));
        }

        if (!string.IsNullOrEmpty(metadata.Source))
            writer.WriteLine(Invariant($"&source={metadata.Source}"));


        // We repeat the author field, the description is used by simai to include additional info. Chart author is one of them.
        writer.WriteLine(Invariant($"&author={metadata.Author.Username}"));
        writer.WriteLine(Invariant($"&des={metadata.Author.Username}"));

        // Encode tags in-case someone decides to import  a simai chart encoded by sentakki
        if (!string.IsNullOrEmpty(beatmap.Metadata.Tags)) writer.WriteLine(Invariant($"&tags={beatmap.Metadata.Tags}"));

        writer.WriteLine(Invariant($"&wholebpm={(int)double.Round(beatmap.BeatmapInfo.BPM)}"));

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
        var hitObjectsGroups = beatmap.HitObjects.GroupBy(h => h.StartTime).OrderBy(g => g.Key).ToList();

        if (hitObjectsGroups.Count == 0)
            return "E";

        StringBuilder maidataBuilder = new();

        // Add padding timingPoint prior to first hitobject
        if (hitObjectsGroups[0].Key > 0)
            maidataBuilder.Append($"\n{{#{hitObjectsGroups[0].Key / 1000:F3}}},");

        for (int i = 0; i < hitObjectsGroups.Count; ++i)
        {
            var group = hitObjectsGroups[i];

            if (i < hitObjectsGroups.Count - 1)
            {
                var nextGroup = hitObjectsGroups[i + 1];
                double delta = nextGroup.Key - group.Key;

                maidataBuilder.Append($"\n{{#{delta / 1000:F3}}}");
            }

            var hitobjects = group.ToList();
            for (int j = 0; j < hitobjects.Count; ++j)
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
                if (j < hitobjects.Count - 1)
                    maidataBuilder.Append('/');
            }

            maidataBuilder.Append(',');
        }

        return maidataBuilder.ToString();
    }

    protected static string TapToString(Tap tap) => $"{tap.Lane + 1}{(tap.Break ? "b" : "")}{(tap.Ex ? "x" : "")}";
    protected static string HoldToString(Hold hold) => $"{hold.Lane + 1}h{(hold.Break ? "b" : "")}{(hold.Ex ? "x" : "")}[#{hold.Duration / 1000:F3}]";

    protected string SlideToString(Slide slide)
    {
        StringBuilder slideBuilder = new();

        slideBuilder.Append($"{slide.Lane + 1}");

        // Tap has break
        if (slide.Break)
            slideBuilder.Append('b');

        // Tap no star
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
                    int endLane = (currentLane + part.EndOffset).NormalizePath();
                    slideBuilder.Append(shapeForSlidePart(currentLane, part));
                    slideBuilder.Append(endLane + 1);
                    currentLane = endLane;
                }

                double millisPerBeat = beatmap.ControlPointInfo.TimingPointAt(slide.StartTime).BeatLength;
                double shootDelayMs = slideInfo.ShootDelay * millisPerBeat;

                double durationWithoutDelay = slideInfo.Duration - shootDelayMs;

                slideBuilder.Append($"[{shootDelayMs / 1000:F3}##{durationWithoutDelay / 1000:F3}]");
                if (slideInfo.Break)
                    slideBuilder.Append('b');

                if (i < slide.SlideInfoList.Count - 1)
                    slideBuilder.Append('*');
            }
        }

        return slideBuilder.ToString();
    }

    private static string shapeForSlidePart(int startLane, in SlideBodyPart part)
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
                return "w";

            default:
                return "-";
        }
    }

    protected static string TouchToString(Touch touch) => TouchPositionMapping.MinBy(kv =>
                                                    {
                                                        double xDelta = Math.Abs(kv.Key.X - touch.Position.X);
                                                        double yDelta = Math.Abs(kv.Key.Y - touch.Position.Y);
                                                        return Math.Sqrt(xDelta * xDelta + yDelta * yDelta);
                                                    }).Value;

    protected static string TouchHoldToString(TouchHold touchHold) => $"C[#{touchHold.Duration / 1000:F3}]";

    public void SerializeToFile()
    {
        var metadata = beatmap.BeatmapInfo.Metadata;
        string path = $"(sen) {metadata.ArtistUnicode} - {metadata.TitleUnicode} ({beatmap.BeatmapInfo.DifficultyName}).txt";
        var file = File.CreateText(path);
        Encode(file);
        file.Close();
    }
}
