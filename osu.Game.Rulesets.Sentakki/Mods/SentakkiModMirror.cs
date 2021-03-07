using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Mods;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Framework.Bindables;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModMirror : Mod, IApplicableAfterBeatmapConversion
    {
        public override string Name => "Mirror";
        public override string Acronym => "MR";
        public override ModType Type => ModType.Conversion;
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;

        public override bool RequiresConfiguration => true;

        [SettingSource("Mirror along X Axis")]
        public BindableBool XAxis { get; } = new BindableBool
        {
            Default = false,
            Value = false
        };

        [SettingSource("Mirror along Y Axis")]
        public BindableBool YAxis { get; } = new BindableBool
        {
            Default = false,
            Value = false
        };

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            //Mirroring time
            bool mirrored = XAxis.Value ^ YAxis.Value;
            beatmap.HitObjects.OfType<SentakkiLanedHitObject>().ForEach(laned =>
            {
                if (YAxis.Value)
                {
                    laned.Lane = 7 - laned.Lane;
                }

                if (XAxis.Value)
                {
                    laned.Lane = (3 - laned.Lane) % 8;
                    if (laned.Lane < 0) laned.Lane += 8;
                }

                if (mirrored && laned is Slide slide)
                    slide.SlideInfoList.ForEach(slideInfo => slideInfo.Mirrored ^= mirrored);
            });

            beatmap.HitObjects.OfType<Touch>().ForEach(touch =>
            {
                Vector2 newPosition = touch.Position;
                if (YAxis.Value)
                    newPosition.X = -touch.Position.X;

                if (XAxis.Value)
                    newPosition.Y = -touch.Position.Y;

                touch.Position = newPosition;
            });
        }
    }
}
