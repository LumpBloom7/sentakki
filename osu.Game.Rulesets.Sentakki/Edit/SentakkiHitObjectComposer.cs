using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Edit.CompositionTools;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    [Cached]
    public partial class SentakkiHitObjectComposer : HitObjectComposer<SentakkiHitObject>
    {
        public new DrawableSentakkiRuleset DrawableRuleset => (DrawableSentakkiRuleset)base.DrawableRuleset;

        [Cached]
        private SentakkiSnapProvider snapProvider { get; set; } = new SentakkiSnapProvider();

        protected override Drawable CreateHitObjectInspector() => new SentakkiHitObjectInspector();

        public SentakkiSelectionHandler SelectionHandler => (SentakkiSelectionHandler)BlueprintContainer.SelectionHandler;

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
                   .Concat(createHitObjectFlagTernaries())
                   .Concat(snapProvider.CreateTernaryButtons());

        public SnapResult FindSnappedPositionAndTime(Vector2 screenSpacePosition)
            => snapProvider.GetSnapResult(screenSpacePosition);

        protected override ComposeBlueprintContainer CreateBlueprintContainer() => new SentakkiBlueprintContainer(this);

        private BindableList<HitObject> selectedHitObjects = null!;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            RightToolbox.Add(slideEditorToolboxGroup);
            RightToolbox.Add(new TransformToolboxGroup
            {
                RotationHandler = BlueprintContainer.SelectionHandler.RotationHandler,
            });

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

        private IEnumerable<DrawableTernaryButton> createHitObjectFlagTernaries()
        {
            var selectionHandler = (SentakkiSelectionHandler)BlueprintContainer.SelectionHandler;
            yield return new DrawableTernaryButton
            {
                Current = selectionHandler.SelectionBreakState,
                Description = "Break",
                CreateIcon = () => new SpriteIcon { Icon = FontAwesome.Solid.WeightHanging }
            };
            yield return new DrawableTernaryButton
            {
                Current = selectionHandler.SelectionExState,
                Description = "Ex",
                CreateIcon = () => new SpriteIcon { Icon = FontAwesome.Solid.Seedling }
            };
        }

        private CompositionTool? lastTool = null;

        public void updateSnapProvider()
        {
            lastTool = BlueprintContainer.CurrentTool;

            if (BlueprintContainer.CurrentTool is SelectTool)
            {
                if (selectedHitObjects.Count == 0)
                    snapProvider.SwitchModes(SentakkiSnapProvider.SnapMode.Off);
                else if (selectedHitObjects.All(h => h is Touch or TouchHold))
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
                TouchHoldCompositionTool => SentakkiSnapProvider.SnapMode.Touch,
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
