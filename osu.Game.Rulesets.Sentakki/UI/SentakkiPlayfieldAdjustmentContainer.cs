// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class SentakkiPlayfieldAdjustmentContainer : PlayfieldAdjustmentContainer
    {
        protected override Container<Drawable> Content => content;
        private readonly Container content;

        public SentakkiPlayfieldAdjustmentContainer()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Size = new Vector2(.8f);

            InternalChild = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                FillAspectRatio = 1,
                Child = content = new ScalingContainer { RelativeSizeAxes = Axes.Both }
            };
        }

        /// <summary>
        /// A <see cref="Container"/> which scales its content relative to a target width.
        /// </summary>
        private class ScalingContainer : Container
        {
            protected override void Update()
            {
                base.Update();
                Scale = new Vector2(Parent.ChildSize.X / 600);
                Size = Vector2.Divide(Vector2.One, Scale);
            }
        }
    }
}