using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Edit.CompositionTools;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Sentakki.Edit;

[Cached]
public partial class SentakkiHitObjectComposer : HitObjectComposer<SentakkiHitObject>
{
    public new DrawableSentakkiRuleset DrawableRuleset => (DrawableSentakkiRuleset)base.DrawableRuleset;

    public SentakkiHitObjectComposer(Ruleset ruleset)
        : base(ruleset)
    {
    }

    private DrawableRulesetDependencies dependencies = null!;

    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        => dependencies = new DrawableRulesetDependencies(Ruleset, base.CreateChildDependencies(parent));

    protected override ComposeBlueprintContainer CreateBlueprintContainer()
        => new SentakkiBlueprintContainer(this);

    protected override DrawableRuleset<SentakkiHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
    => new DrawableSentakkiEditorRuleset((SentakkiRuleset)ruleset, beatmap, mods);

    [Cached]
    public TouchPositionSnapGrid TouchPositionSnapGrid { get; private set; } = new TouchPositionSnapGrid();

    [Cached]
    public LaneNoteSnapGrid LaneNoteSnapGrid { get; private set; } = new LaneNoteSnapGrid();

    [BackgroundDependencyLoader]
    private void load()
    {
        LayerBelowRuleset.Add(TouchPositionSnapGrid);
        LayerBelowRuleset.Add(LaneNoteSnapGrid);

        EditorBeatmap.SelectedHitObjects.CollectionChanged += (_, _) => UpdateSnapGrid();
    }

    private CompositionTool? currentTool = null;

    protected override void Update()
    {
        base.Update();

        if (BlueprintContainer.CurrentTool != currentTool)
        {
            currentTool = BlueprintContainer.CurrentTool;

            UpdateSnapGrid();
        }
    }

    public void UpdateSnapGrid()
    {
        TouchPositionSnapGrid.Hide();
        LaneNoteSnapGrid.Hide();

        switch (BlueprintContainer.CurrentTool)
        {
            case SelectTool:
                if (EditorBeatmap.SelectedHitObjects.Count == 0)
                    break;

                if (EditorBeatmap.SelectedHitObjects.All(h => h is IHasPosition))
                    TouchPositionSnapGrid.Show();
                if (EditorBeatmap.SelectedHitObjects.All(h => h is SentakkiLanedHitObject))
                    LaneNoteSnapGrid.Show();
                break;

            case TapCompositionTool:
            case HoldCompositionTool:
            case SlideCompositionTool:
                LaneNoteSnapGrid.Show();
                break;

            case TouchCompositionTool:
            case TouchHoldCompositionTool:
                TouchPositionSnapGrid.Show();
                break;
        }
    }

    protected override IEnumerable<Drawable> CreateTernaryButtons()
    {
        foreach (var ternaryButton in base.CreateTernaryButtons().Skip(1))
            yield return ternaryButton;

        var selectionHandler = (SentakkiSelectionHandler)BlueprintContainer.SelectionHandler;

        yield return new DrawableTernaryButton
        {
            Current = selectionHandler.BreakTernaryState,
            CreateIcon = () => new SpriteIcon { Icon = FontAwesome.Solid.WeightHanging },
            Description = "Break",
            TooltipText = "Increases the scoring weight of notes. Typically used to emphasize certain notes, or to increase punishment for inaccuracy."
        };

        yield return new DrawableTernaryButton
        {
            Current = selectionHandler.ExTernaryState,
            CreateIcon = () => new SpriteIcon { Icon = FontAwesome.Solid.Seedling },
            Description = "Ex",
            TooltipText = "Increases the judgement leniency of notes. Typically used to provide a safety net for players, allowing harder patterns to be introduced."
        };
    }

    protected override IReadOnlyList<CompositionTool> CompositionTools { get; } =
    [
        new TapCompositionTool(),
        new HoldCompositionTool(),
        new SlideCompositionTool(),
        new TouchCompositionTool(),
        new TouchHoldCompositionTool(),
    ];

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);

        dependencies?.Dispose();
    }
}
