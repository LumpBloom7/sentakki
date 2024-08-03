using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touches;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public partial class SentakkiBlueprintContainer : ComposeBlueprintContainer
    {
        public SentakkiBlueprintContainer(HitObjectComposer composer)
            : base(composer)
        {
            AddInternal(chevronPool = new DrawablePool<SlideChevron>(100));
            AddInternal(fanChevrons = new SlideFanChevrons());
        }

        [Cached]
        private DrawablePool<SlideChevron> chevronPool;

        [Cached]
        private SlideFanChevrons fanChevrons;

        protected override SelectionHandler<HitObject> CreateSelectionHandler() => new SentakkiSelectionHandler();

        protected override bool ApplySnapResult(SelectionBlueprint<HitObject>[] blueprints, SnapResult result)
        {
            if (!base.ApplySnapResult(blueprints, result))
                return false;

            if (blueprints.All(b => b.Item is SentakkiLanedHitObject))
            {
                SentakkiLanedSnapResult senSnapResult = (SentakkiLanedSnapResult)result;

                int offset = senSnapResult.Lane - ((SentakkiLanedHitObject)blueprints.First().Item).Lane;
                if (offset != 0)
                {
                    Beatmap.PerformOnSelection(delegate (HitObject ho)
                    {
                        var lho = (SentakkiLanedHitObject)ho;

                        lho.Lane = (lho.Lane + offset).NormalizePath();
                        Beatmap.Update(ho);
                    });
                }
            }

            return true;
        }

        public override HitObjectSelectionBlueprint CreateHitObjectBlueprintFor(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Tap t:
                    return new TapSelectionBlueprint(t);

                case Hold h:
                    return new HoldSelectionBlueprint(h);

                case Touch t:
                    return new TouchSelectionBlueprint(t);

                case TouchHold th:
                    return new TouchHoldSelectionBlueprint(th);

                case Slide s:
                    return new SlideSelectionBlueprint(s);
            }

            return base.CreateHitObjectBlueprintFor(hitObject);
        }

        private Vector2 currentMousePosition => InputManager.CurrentState.Mouse.Position;

        protected override IEnumerable<SelectionBlueprint<HitObject>> SortForMovement(IReadOnlyList<SelectionBlueprint<HitObject>> blueprints)
            => blueprints.OrderBy(b => Vector2.DistanceSquared(b.ScreenSpaceSelectionPoint, currentMousePosition));
    }
}
