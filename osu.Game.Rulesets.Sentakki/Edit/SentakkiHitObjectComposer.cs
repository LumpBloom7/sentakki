using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Objects;
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
        private SentakkiSnapProvider snapProvider { get; set; } = new SentakkiSnapProvider();

        public SentakkiHitObjectComposer(SentakkiRuleset ruleset)
            : base(ruleset)
        {
        }

        private DrawableRulesetDependencies dependencies = null!;

        [Cached]
        private SlideEditorToolboxGroup slideEditorToolboxGroup = new SlideEditorToolboxGroup();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DrawableRulesetDependencies(Ruleset, base.CreateChildDependencies(parent));

        protected override IReadOnlyList<CompositionTool> CompositionTools =>
        [
            new TapCompositionTool(),
            new HoldCompositionTool(),
            new TouchCompositionTool(),
            new TouchHoldCompositionTool(),
            new SlideCompositionTool(),
        ];

        protected override IEnumerable<Drawable> CreateTernaryButtons()
            => base.CreateTernaryButtons()
                    .Skip(1)
                    .Concat(snapProvider.CreateTernaryButtons());


        public override SnapResult FindSnappedPositionAndTime(Vector2 screenSpacePosition, SnapType snapType = SnapType.All)
            => snapProvider.GetSnapResult(screenSpacePosition);


        protected override ComposeBlueprintContainer CreateBlueprintContainer() => new SentakkiBlueprintContainer(this);

        private BindableList<HitObject> selectedHitObjects = null!;


        [BackgroundDependencyLoader]
        private void load()
        {
            RightToolbox.Add(slideEditorToolboxGroup);
            LayerBelowRuleset.Add(snapProvider);

            selectedHitObjects = EditorBeatmap.SelectedHitObjects.GetBoundCopy();
            selectedHitObjects.CollectionChanged += (_, _) => updateSnapProvider();
        }

        protected override void Update()
        {
            base.Update();

            if (BlueprintContainer.CurrentTool != lastTool)
            {
                lastTool = BlueprintContainer.CurrentTool;
                updateSnapProvider();
            }
        }

        private CompositionTool? lastTool = null;

        public void updateSnapProvider()
        {
            lastTool = BlueprintContainer.CurrentTool;
            if (BlueprintContainer.CurrentTool is SelectTool)
            {
                if (selectedHitObjects.Count == 0)
                    snapProvider.SwitchModes(SentakkiSnapProvider.SnapMode.Off);
                else if (selectedHitObjects.All(h => h is Touch))
                    snapProvider.SwitchModes(SentakkiSnapProvider.SnapMode.Touch);
                else if (selectedHitObjects.All(h => h is SentakkiLanedHitObject))
                    snapProvider.SwitchModes(SentakkiSnapProvider.SnapMode.Laned);
                else
                    snapProvider.SwitchModes(SentakkiSnapProvider.SnapMode.Off);
                return;
            }

            snapProvider.SwitchModes(BlueprintContainer.CurrentTool switch
            {
                TouchCompositionTool => SentakkiSnapProvider.SnapMode.Touch,
                TouchHoldCompositionTool => SentakkiSnapProvider.SnapMode.Off,
                _ => SentakkiSnapProvider.SnapMode.Laned,
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            dependencies?.Dispose();
        }
    }
}
