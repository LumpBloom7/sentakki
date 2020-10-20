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

        protected SentakkiSelectionBlueprint(DrawableHitObject drawableObject)
            : base(drawableObject)
        {
        }
    }
}
