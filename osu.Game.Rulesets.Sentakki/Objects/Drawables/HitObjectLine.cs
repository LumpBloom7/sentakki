// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class HitObjectLine : CircularContainer
    {
        public HitObjectLine()
        {
            Size = new Vector2(SentakkiPlayfield.NoteStartDistance * 2);
            Masking = true;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Alpha = 0;
            Child = new CircularProgress
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
