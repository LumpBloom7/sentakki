// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints
{
    public abstract class SentakkiSelectionBlueprint<T> : OverlaySelectionBlueprint
        where T : SentakkiHitObject
    {
        protected new T HitObject => (T)DrawableObject.HitObject;

        protected override bool AlwaysShowWhenSelected => true;

        protected override bool ShouldBeAlive =>
            (DrawableObject.IsAlive && DrawableObject.IsPresent && (DrawableObject as DrawableSentakkiHitObject).IsVisible) || (AlwaysShowWhenSelected && State == SelectionState.Selected);

        protected SentakkiSelectionBlueprint(DrawableHitObject drawableObject)
            : base(drawableObject)
        {
        }
    }
}