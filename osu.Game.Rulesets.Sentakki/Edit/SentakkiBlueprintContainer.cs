using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class SentakkiBlueprintContainer : ComposeBlueprintContainer
    {
        public SentakkiBlueprintContainer(HitObjectComposer composer)
            : base(composer)
        {
        }

        public override OverlaySelectionBlueprint CreateBlueprintFor(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableTap t:
                    return new TapSelectionBlueprint(t);
                case DrawableHold h:
                    return new HoldSelectionBlueprint(h);
            }
            return base.CreateBlueprintFor(hitObject);
        }
    }
}
