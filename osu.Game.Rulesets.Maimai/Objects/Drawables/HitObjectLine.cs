// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets.Maimai.Configuration;
using osu.Game.Rulesets.Maimai.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Maimai.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;
using System;
using System.Linq;

namespace osu.Game.Rulesets.Maimai.Objects.Drawables
{
    public class HitObjectLine : CircularContainer
    {
        public CircularProgress Line;
        public HitObjectLine()
        {
            Size = new Vector2(MaimaiPlayfield.NoteStartDistance * 2);
            Masking = true;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Alpha = 0;
            Child = Line = new CircularProgress
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                InnerRadius = 0.025f,
                Rotation = -45,
                Current = new Bindable<double>(0.25),
            };
        }
    }
}
