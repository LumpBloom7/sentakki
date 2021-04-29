using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps
{
    public class SentakkiSelectionBlueprint : OverlaySelectionBlueprint
    {
        protected override bool AlwaysShowWhenSelected => true;

        public SentakkiSelectionBlueprint(DrawableHitObject dho) : base(dho) { }

        protected override bool ShouldBeAlive
        {
            get
            {
                if (DrawableObject.Time.Current < DrawableObject.LifetimeStart || DrawableObject.Time.Current >= DrawableObject.LifetimeEnd)
                {
                    if (AlwaysShowWhenSelected)
                        return State == SelectionState.Selected;
                    return false;
                }
                return true;
            }
        }
    }
}
