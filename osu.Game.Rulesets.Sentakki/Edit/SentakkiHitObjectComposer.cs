using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public partial class SentakkiHitObjectComposer : HitObjectComposer<SentakkiHitObject>
    {
        [Cached]
        private SentakkiSnapGrid snapGrid { get; set; } = new SentakkiSnapGrid();

        public SentakkiHitObjectComposer(SentakkiRuleset ruleset)
            : base(ruleset)
        {
        }

        private DrawableRulesetDependencies dependencies = null!;

        [Cached]
        private SlideEditorToolboxGroup slideEditorToolboxGroup = new SlideEditorToolboxGroup();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DrawableRulesetDependencies(Ruleset, base.CreateChildDependencies(parent));

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new HitObjectCompositionTool[]
        {
            new TapCompositionTool(),
            new HoldCompositionTool(),
            new TouchCompositionTool(),
            new TouchHoldCompositionTool(),
            new SlideCompositionTool(),
        };

        protected override IEnumerable<TernaryButton> CreateTernaryButtons() => base.CreateTernaryButtons().Skip(1);

        public override SnapResult FindSnappedPositionAndTime(Vector2 screenSpacePosition, SnapType snapType = SnapType.All)
        {
            return snapGrid.GetSnapResult(screenSpacePosition);
        }

        protected override ComposeBlueprintContainer CreateBlueprintContainer() => new SentakkiBlueprintContainer(this);

        [BackgroundDependencyLoader]
        private void load()
        {
            RightToolbox.Add(slideEditorToolboxGroup);
            LayerBelowRuleset.Add(snapGrid);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            dependencies?.Dispose();
        }
    }
}
