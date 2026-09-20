using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Edit.CompositionTools;
using osu.Game.Rulesets.Sentakki.Edit.Inspector;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Sentakki.Edit;

[Cached]
public partial class SentakkiHitObjectComposer : HitObjectComposer<SentakkiHitObject, SentakkiAction>
{
    public new DrawableSentakkiRuleset DrawableRuleset => (DrawableSentakkiRuleset)base.DrawableRuleset;

    public SentakkiHitObjectComposer(Ruleset ruleset)
        : base(ruleset)
    {
    }

    private DrawableRulesetDependencies dependencies = null!;

    protected override Drawable CreateHitObjectInspector() => new SentakkiHitObjectInspector();

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
        setupTernaryBindables();
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
                if (EditorBeatmap.SelectedHitObjects.Any(h => h is SentakkiLanedHitObject))
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

    protected override IReadOnlyList<CompositionTool<SentakkiAction>> CompositionTools { get; } =
    [
        new TapCompositionTool(),
        new HoldCompositionTool(),
        new SlideCompositionTool(),
        new TouchCompositionTool(),
        new TouchHoldCompositionTool(),
    ];

    #region Selection ternary states

    public override Bindable<TernaryState>? SelectionNewComboState => null;

    public IBindable<TernaryState> ExTernaryState => exTernaryState;
    private readonly Bindable<TernaryState> exTernaryState = new Bindable<TernaryState>();

    public IBindable<TernaryState> BreakTernaryState => breakTernaryState;
    private readonly Bindable<TernaryState> breakTernaryState = new Bindable<TernaryState>();

    public IBindable<TernaryState> ExSlideTernaryState => exSlideTernaryState;
    private readonly Bindable<TernaryState> exSlideTernaryState = new Bindable<TernaryState>();

    public IBindable<TernaryState> BreakSlideTernaryState => breakSlideTernaryState;
    private readonly Bindable<TernaryState> breakSlideTernaryState = new Bindable<TernaryState>();

    public IBindable<TernaryState> OmitSlideTapTernaryState => omitSlideTapTernaryState;
    private readonly Bindable<TernaryState> omitSlideTapTernaryState = new Bindable<TernaryState>();

    private void setupTernaryBindables()
    {
        exTernaryState.ValueChanged += v => applyTernaryChanges<SentakkiHitObject>(setExState, v.NewValue);
        breakTernaryState.ValueChanged += v => applyTernaryChanges<SentakkiHitObject>(setBreakState, v.NewValue);

        exSlideTernaryState.ValueChanged += v => applyTernaryChanges<Slide>(setExSlideState, v.NewValue);
        breakSlideTernaryState.ValueChanged += v => applyTernaryChanges<Slide>(setBreakSlideState, v.NewValue);

        omitSlideTapTernaryState.ValueChanged += v => applyTernaryChanges<Slide>(setOmitSlideTapState, v.NewValue);
    }

    protected override IEnumerable<Drawable> CreateTernaryButtons()
    {
        foreach (var ternaryButton in base.CreateTernaryButtons().Skip(1))
            yield return ternaryButton;

        var selectionHandler = (SentakkiSelectionHandler)BlueprintContainer.SelectionHandler;

        yield return new DrawableTernaryButton
        {
            Current = breakTernaryState,
            CreateIcon = () => new SpriteIcon { Icon = FontAwesome.Solid.WeightHanging },
            Description = "Break",
            TooltipText = "Increases the scoring weight of notes. Typically used to emphasize certain notes, or to increase punishment for inaccuracy."
        };

        yield return new DrawableTernaryButton
        {
            Current = exTernaryState,
            CreateIcon = () => new SpriteIcon { Icon = FontAwesome.Solid.Seedling },
            Description = "Ex",
            TooltipText = "Increases the judgement leniency of notes. Typically used to provide a safety net for players, allowing harder patterns to be introduced."
        };

        yield return new DrawableTernaryButton()
        {
            Current = LaneNoteSnapGrid.Enabled,
            CreateIcon = () => new SpriteIcon { Icon = OsuIcon.EditorDistanceSnap },
            Description = "Lane note snap grid",
        };

        yield return new DrawableTernaryButton()
        {
            Current = TouchPositionSnapGrid.Enabled,
            CreateIcon = () => new SpriteIcon { Icon = OsuIcon.EditorGridSnap },
            Description = "Touch snap grid",
        };
    }

    protected override void UpdateTernaryStates()
    {
        base.UpdateTernaryStates();

        var selectedItems = EditorBeatmap.SelectedHitObjects.OfType<SentakkiHitObject>().ToList();
        exTernaryState.Value = selectedItems.Where(h => h is not TouchHold).GetTernaryState(h => h.Ex);
        breakTernaryState.Value = selectedItems.GetTernaryState(h => h.Break);

        var selectedSlideBodies = selectedItems.OfType<Slide>().SelectMany(s => s.SlideInfoList);
        breakSlideTernaryState.Value = selectedSlideBodies.GetTernaryState(s => s.Break);
        exSlideTernaryState.Value = selectedSlideBodies.GetTernaryState(s => s.Ex);

        var selectedSlides = selectedItems.OfType<Slide>();
        omitSlideTapTernaryState.Value = selectedSlides.GetTernaryState(s => s.TapType == Slide.TapTypeEnum.None);
    }

    private void applyTernaryChanges<T>(Func<T, bool, bool> applicator, TernaryState newTernaryState) where T : HitObject
    {
        // We can get into an indeterminate state when mixing notes with different break/ex values
        // We don't want to force enable/disable from this intermediate state
        if (newTernaryState is TernaryState.Indeterminate)
            return;

        var selectedItems = EditorBeatmap.SelectedHitObjects.OfType<T>().ToArray();

        bool newValue = newTernaryState is TernaryState.True;

        EditorBeatmap.BeginChange();

        foreach (var item in selectedItems)
        {
            if (applicator(item, newValue))
                EditorBeatmap.Update(item);
        }

        EditorBeatmap.EndChange();
    }

    private bool setExState(SentakkiHitObject hitObject, bool newValue)
    {
        if (hitObject.Ex == newValue)
            return false;

        hitObject.Ex = newValue;
        return true;
    }

    private bool setBreakState(SentakkiHitObject hitObject, bool newValue)
    {
        if (hitObject.Break == newValue)
            return false;

        hitObject.Break = newValue;
        return true;
    }

    private bool setOmitSlideTapState(Slide slide, bool newValue)
    {
        var newState = newValue ? Slide.TapTypeEnum.None : Slide.TapTypeEnum.Star;

        if (slide.TapType == newState)
            return false;

        slide.TapType = newState;

        // Revalidate the arcs
        Playfield.Remove(slide);
        Playfield.Add(slide);

        return true;
    }

    private bool setExSlideState(Slide slide, bool newValue)
    {
        bool anySet = false;

        foreach (var slideInfo in slide.SlideInfoList)
        {
            if (slideInfo.Ex == newValue)
                continue;

            anySet = true;
            slideInfo.Ex = newValue;
        }

        return anySet;
    }

    private bool setBreakSlideState(Slide slide, bool newValue)
    {
        bool anySet = false;

        foreach (var slideInfo in slide.SlideInfoList)
        {
            if (slideInfo.Break == newValue)
                continue;

            anySet = true;
            slideInfo.Break = newValue;
        }

        return anySet;
    }

    #endregion

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);

        dependencies?.Dispose();
    }
}
