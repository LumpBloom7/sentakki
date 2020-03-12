// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Maimai.Objects;
using osu.Game.Rulesets.Maimai.UI;
using osuTK;
using osuTK.Graphics;
using System;

namespace osu.Game.Rulesets.Maimai.Beatmaps
{
    public class MaimaiBeatmapConverter : BeatmapConverter<MaimaiHitObject>
    {

        // todo: Check for conversion types that should be supported (ie. Beatmap.HitObjects.Any(h => h is IHasXPosition))
        // https://github.com/ppy/osu/tree/master/osu.Game/Rulesets/Objects/Types
        public override bool CanConvert() => Beatmap.HitObjects.All(h => h is IHasPosition);

        public MaimaiBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
        }
        protected override IEnumerable<MaimaiHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap)
        {
            Vector2 CENTRE_POINT = new Vector2(256, 192);
            Vector2 newPos = (original as IHasPosition)?.Position ?? Vector2.Zero;
            newPos.Y = 384 - newPos.Y;
            float Angle_ = Utils.GetNotePathFromDegrees(Utils.GetDegreesFromPosition(newPos, CENTRE_POINT));

            yield return new MaimaiHitObject
            {
                NoteColor = Color4.Orange,
                Angle = Angle_,
                Samples = original.Samples,
                StartTime = original.StartTime,
                endPosition = new Vector2(-(MaimaiPlayfield.intersectDistance * (float)Math.Cos((Angle_ + 90f) * (float)(Math.PI / 180))), -(MaimaiPlayfield.intersectDistance * (float)Math.Sin((Angle_ + 90f) * (float)(Math.PI / 180)))),
                Position = new Vector2(-(MaimaiPlayfield.noteStartDistance * (float)Math.Cos((Angle_ + 90f) * (float)(Math.PI / 180))), -(MaimaiPlayfield.noteStartDistance * (float)Math.Sin((Angle_ + 90f) * (float)(Math.PI / 180)))),
            };
        }
    }
}
