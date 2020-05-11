// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSentakkiHitObject : DrawableHitObject<SentakkiHitObject>
    {
        public bool IsHidden = false;
        public bool IsFadeIn = false;

        protected override float SamplePlaybackPosition => (HitObject.EndPosition.X + SentakkiPlayfield.INTERSECTDISTANCE) / (SentakkiPlayfield.INTERSECTDISTANCE * 2);
        public SentakkiAction[] HitActions { get; set; } = new[]
        {
            SentakkiAction.Button1,
            SentakkiAction.Button2,
        };

        public DrawableSentakkiHitObject(SentakkiHitObject hitObject)
            : base(hitObject)
        {
        }

        private DrawableSentakkiRuleset drawableSentakkiRuleset;

        public double GameplaySpeed => drawableSentakkiRuleset?.GameplaySpeed ?? 1;

        [BackgroundDependencyLoader(true)]
        private void load(DrawableSentakkiRuleset drawableRuleset)
        {
            drawableSentakkiRuleset = drawableRuleset;
        }
    }
}
