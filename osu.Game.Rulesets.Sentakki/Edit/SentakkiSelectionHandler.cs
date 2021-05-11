using System.Collections;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class SentakkiSelectionHandler : EditorSelectionHandler
    {
        private readonly Bindable<TernaryState> selectionBreakState = new Bindable<TernaryState>();
        private readonly Bindable<TernaryState> selectionSlideMirroredState = new Bindable<TernaryState>();

        public SentakkiSelectionHandler()
        {
            selectionBreakState.ValueChanged += s =>
            {
                switch (s.NewValue)
                {
                    case TernaryState.False:
                    case TernaryState.True:
                        setBreakState(s.NewValue == TernaryState.True);
                        break;
                }
            };

            selectionSlideMirroredState.ValueChanged += s =>
            {
                switch (s.NewValue)
                {
                    case TernaryState.False:
                    case TernaryState.True:
                        setMirroredState(s.NewValue == TernaryState.True);
                        break;
                }
            };
        }
        public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent)
        {
            if (SelectedBlueprints.Count > 1)
                return false;

            switch (moveEvent.Blueprint.Item)
            {
                case SentakkiLanedHitObject laned:
                {
                    var CursorPosition = ToLocalSpace(moveEvent.Blueprint.ScreenSpaceSelectionPoint + moveEvent.ScreenSpaceDelta) - new Vector2(300, 300);
                    var currentAngle = Vector2.Zero.GetDegreesFromPosition(CursorPosition);
                    laned.Lane = currentAngle.GetNoteLaneFromDegrees();

                    break;
                }
                case Touch t:
                    Vector2 HitObjectPosition = t.Position;
                    HitObjectPosition += this.ScreenSpaceDeltaToParentSpace(moveEvent.ScreenSpaceDelta);

                    if (Vector2.Distance(Vector2.Zero, HitObjectPosition) > 250)
                    {
                        var currentAngle = Vector2.Zero.GetDegreesFromPosition(HitObjectPosition);
                        HitObjectPosition = SentakkiExtensions.GetCircularPosition(250, currentAngle);
                    }

                    t.Position = HitObjectPosition;
                    break;
            }
            return true;
        }

        private void setBreakState(bool state)
        {
            var lhos = EditorBeatmap.SelectedHitObjects.OfType<SentakkiLanedHitObject>();

            EditorBeatmap.BeginChange();

            foreach (var lho in lhos)
            {
                if (lho.Break == state)
                    continue;

                lho.Break = state;
                EditorBeatmap.Update(lho);
            }

            EditorBeatmap.EndChange();
        }
        private void setMirroredState(bool state)
        {
            var lhos = EditorBeatmap.SelectedHitObjects.OfType<Slide>();

            EditorBeatmap.BeginChange();

            foreach (var lho in lhos)
            {
                if (lho.SlideInfoList.First().Mirrored == state)
                    continue;

                lho.SlideInfoList.First().Mirrored = state;
                EditorBeatmap.Update(lho);
            }

            EditorBeatmap.EndChange();
        }

        protected override void UpdateTernaryStates()
        {
            base.UpdateTernaryStates();

            selectionBreakState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<SentakkiLanedHitObject>(), h => h.Break);
            selectionSlideMirroredState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<Slide>(), h => h.SlideInfoList.First().Mirrored);
        }

        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint<HitObject>> selection)
        {
            if (selection.Any(s => s.Item is SentakkiLanedHitObject))
                yield return new TernaryStateMenuItem("Break") { State = { BindTarget = selectionBreakState } };

            if (selection.All(s => s.Item is Slide))
            {
                yield return new TernaryStateMenuItem("Mirrored") { State = { BindTarget = selectionSlideMirroredState } };
                yield return new OsuMenuItem("Patterns") { Items = getContextMenuItemsForSlide() };
            }

            foreach (var item in base.GetContextMenuItemsForSelection(selection))
                yield return item;
        }

        private List<MenuItem> getContextMenuItemsForSlide()
        {
            var patterns = new List<MenuItem>();

            var SectionItems = new List<OsuMenuItem>();
            void createPatternGroup(string patternName)
                => patterns.Add(new OsuMenuItem(patternName) { Items = SectionItems = new List<OsuMenuItem>() });

            for (int i = 0; i < SlidePaths.VALIDPATHS.Count; ++i)
            {
                if (i == 0)
                    createPatternGroup("Circular");
                else if (i == 7)
                    createPatternGroup("L shape");
                else if (i == 11)
                    createPatternGroup("Straight");
                else if (i == 14)
                    createPatternGroup("Thunder");
                else if (i == 15)
                    createPatternGroup("U shape");
                else if (i == 23)
                    createPatternGroup("V shape");
                else if (i == 26)
                    createPatternGroup("Cup shape");

                int j = i;
                SectionItems.Add(createMenuEntryForPattern(j));
            }
            return patterns;
        }

        private OsuMenuItem createMenuEntryForPattern(int ID)
        {
            void commit()
            {
                EditorBeatmap.BeginChange();
                foreach (var bp in SelectedBlueprints)
                {
                    (bp.Item as Slide).SlideInfoList.First().ID = ID;
                    EditorBeatmap.Update(bp.Item);
                }
                EditorBeatmap.EndChange();
            };

            return new OsuMenuItem(SlidePaths.VALIDPATHS[ID].Item1.EndLane.ToString(), MenuItemType.Standard, commit);
        }
    }
}
