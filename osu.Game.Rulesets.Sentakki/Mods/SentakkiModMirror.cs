using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sentakki.Localisation.Mods;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModMirror : Mod, IApplicableAfterBeatmapConversion
    {
        public override string Name => "Mirror";
        public override LocalisableString Description => SentakkiModMirrorStrings.ModDescription;
        public override string Acronym => "MR";
        public override ModType Type => ModType.Conversion;
        public override double ScoreMultiplier => 1;

        public override bool RequiresConfiguration => true;

        [SettingSource(typeof(SentakkiModMirrorStrings), nameof(SentakkiModMirrorStrings.MirrorVertically), nameof(SentakkiModMirrorStrings.MirrorVerticallyDescription))]
        public BindableBool VerticalMirrored { get; } = new BindableBool
        {
            Default = false,
            Value = false
        };

        [SettingSource(typeof(SentakkiModMirrorStrings), nameof(SentakkiModMirrorStrings.MirrorHorizontally), nameof(SentakkiModMirrorStrings.MirrorHorizontallyDescription))]
        public BindableBool HorizontalMirrored { get; } = new BindableBool
        {
            Default = false,
            Value = false
        };

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            // Mirroring in both directions at the same time is equivalent to an 180deg rotation
            // Because of that, we wouldn't need to swap slide paths with their mirrored counterpart
            bool mirrored = VerticalMirrored.Value ^ HorizontalMirrored.Value;

            beatmap.HitObjects.OfType<SentakkiLanedHitObject>().ForEach(laned =>
            {
                if (HorizontalMirrored.Value)
                    laned.Lane = 7 - laned.Lane;

                if (VerticalMirrored.Value)
                {
                    laned.Lane = (3 - laned.Lane) % 8;
                    if (laned.Lane < 0) laned.Lane += 8;
                }

                if (mirrored && laned is Slide slide)
                {
                    foreach (var slideInfo in slide.SlideInfoList)
                    {
                        for (int i = 0; i < slideInfo.SlidePathParts.Length; ++i)
                        {
                            ref var part = ref slideInfo.SlidePathParts[i];
                            part.EndOffset = (part.EndOffset * -1).NormalizePath();
                            part.Mirrored ^= mirrored;
                        }

                        slideInfo.UpdatePaths();
                    }
                }
            });

            beatmap.HitObjects.OfType<Touch>().ForEach(touch =>
            {
                Vector2 newPosition = touch.Position;
                if (HorizontalMirrored.Value)
                    newPosition.X = -touch.Position.X;

                if (VerticalMirrored.Value)
                    newPosition.Y = -touch.Position.Y;

                touch.Position = newPosition;
            });
        }
    }
}
