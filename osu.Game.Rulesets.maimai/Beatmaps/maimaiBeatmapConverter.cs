// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.maimai.Objects;
using osuTK;

namespace osu.Game.Rulesets.maimai.Beatmaps
{
    public class maimaiBeatmapConverter : BeatmapConverter<maimaiHitObject>
    {
        protected override IEnumerable<Type> ValidConversionTypes { get; } = new[] { typeof(IHasPosition) };

        public maimaiBeatmapConverter(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        protected override IEnumerable<maimaiHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap)
        {
            yield return new maimaiHitObject
            {
                Samples = original.Samples,
                StartTime = original.StartTime,
                Position = (original as IHasPosition)?.Position ?? Vector2.Zero,
            };
        }
    }
}
