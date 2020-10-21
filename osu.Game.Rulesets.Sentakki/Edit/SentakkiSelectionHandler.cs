using System.Linq;
using System;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using osu.Framework.Graphics;
using System.Collections;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Bindables;
using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class SentakkiSelectionHandler : SelectionHandler
    {
        public override bool HandleMovement(MoveSelectionEvent moveEvent)
        {
            if (SelectedBlueprints.Count() > 1)
                return true;

            foreach (var h in EditorBeatmap.SelectedHitObjects.OfType<SentakkiHitObject>())
            {
                var newPos = ToLocalSpace(moveEvent.ScreenSpacePosition);
                newPos = new Vector2(newPos.X - 300, newPos.Y - 300);

                switch (h)
                {
                    case TouchHold _:
                        continue;
                    case Touch touch:
                    {
                        float angle = Vector2.Zero.GetDegreesFromPosition(newPos);
                        float distance = Math.Clamp(Vector2.Distance(newPos, Vector2.Zero), 0, 200);
                        newPos = SentakkiExtensions.GetCircularPosition(distance, angle);

                        touch.Position = newPos;
                        break;
                    }
                    case SentakkiLanedHitObject lho:
                    {
                        lho.Lane = Vector2.Zero.GetDegreesFromPosition(newPos).GetNoteLaneFromDegrees();
                        break;
                    }
                }
            }
            return true;
        }

        private readonly Bindable<TernaryState> selectionBreakState = new Bindable<TernaryState>();

        [BackgroundDependencyLoader]
        private void load()
        {
            selectionBreakState.ValueChanged += state =>
            {
                switch (state.NewValue)
                {
                    case TernaryState.False:
                        SetBreakState(false);
                        break;

                    case TernaryState.True:
                        SetBreakState(true);
                        break;
                }
            };
        }

        public void SetBreakState(bool state)
        {
            var lhos = EditorBeatmap.SelectedHitObjects.OfType<SentakkiLanedHitObject>();

            EditorBeatmap.BeginChange();

            foreach (var lho in lhos)
            {
                if (lho.Break != state)
                {
                    lho.Break = state;
                    EditorBeatmap.Update(lho);
                }
            }

            EditorBeatmap.EndChange();
        }

        [Resolved]
        private IEditorChangeHandler changeHandler { get; set; }

        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint> selection)
        {
            if (selection.All(s => s.HitObject is SentakkiLanedHitObject))
                yield return new TernaryStateMenuItem("Break") { State = { BindTarget = selectionBreakState } };

            if (selection.All(s => s.HitObject is Slide x && x.SlideInfoList.Count == 1))
            {
                var patternList = new OsuMenuItem("SlidePatterns");

                List<MenuItem> items = new List<MenuItem>();

                for (int i = 0; i < SlidePaths.VALIDPATHS.Count; ++i)
                {
                    int j = i;
                    void commit(TernaryState state)
                    {
                        changeHandler.BeginChange();
                        foreach (var bp in selection)
                        {
                            (bp.HitObject as Slide).SlideInfoList.First().ID = j;
                            EditorBeatmap.Update(bp.HitObject);
                        }
                        changeHandler.EndChange();
                    };

                    items.Add(new TernaryStateMenuItem(i.ToString(), action: commit));
                }

                yield return new OsuMenuItem("Pattern")
                {
                    Items = getSlidePatternsEntries().ToList()
                };
            }
        }
        protected override void UpdateTernaryStates()
        {
            base.UpdateTernaryStates();

            selectionBreakState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<SentakkiLanedHitObject>(), h => h.Break);
        }

        private IEnumerable<OsuMenuItem> getSlidePatternsEntries()
        {
            var SectionItems = new List<OsuMenuItem>();

            for (int i = 0; i < SlidePaths.VALIDPATHS.Count; ++i)
            {
                int j = i;
                SectionItems.Add(createMenuEntryForPattern(j));

                if (i == 7)
                {
                    yield return new OsuMenuItem("Clockwise spin") { Items = SectionItems };
                    SectionItems = new List<OsuMenuItem>();
                }
                else if (i == 15)
                {
                    yield return new OsuMenuItem("Counterclockwise spin") { Items = SectionItems };
                    SectionItems = new List<OsuMenuItem>();
                }
                else if (i == 19)
                {
                    yield return new OsuMenuItem("L shape") { Items = SectionItems };
                    SectionItems = new List<OsuMenuItem>();
                }
                else if (i == 23)
                {
                    yield return new OsuMenuItem("L shape (Mirrored)") { Items = SectionItems };
                    SectionItems = new List<OsuMenuItem>();
                }
                else if (i == 28)
                {
                    yield return new OsuMenuItem("Straight") { Items = SectionItems };
                    SectionItems = new List<OsuMenuItem>();
                }
                else if (i == 29)
                {
                    yield return new OsuMenuItem("Thunder", MenuItemType.Standard, createMenuEntryForPattern(j).Action.Value) { };
                    SectionItems = new List<OsuMenuItem>();
                }
                else if (i == 37)
                {
                    yield return new OsuMenuItem("U shape") { Items = SectionItems };
                    SectionItems = new List<OsuMenuItem>();
                }
                else if (i == 45)
                {
                    yield return new OsuMenuItem("U Shape (Reversed)") { Items = SectionItems };
                    SectionItems = new List<OsuMenuItem>();
                }
                else if (i == 51)
                {
                    yield return new OsuMenuItem("V shape") { Items = SectionItems };
                    SectionItems = new List<OsuMenuItem>();
                }
                else if (i == 59)
                {
                    yield return new OsuMenuItem("Cup shape") { Items = SectionItems };
                    SectionItems = new List<OsuMenuItem>();
                }
                else if (i == 67)
                {
                    yield return new OsuMenuItem("Cup shape (Mirrored)") { Items = SectionItems };
                    SectionItems = new List<OsuMenuItem>();
                }
            }
        }

        private OsuMenuItem createMenuEntryForPattern(int ID)
        {
            void commit()
            {
                changeHandler.BeginChange();
                foreach (var bp in SelectedBlueprints)
                {
                    (bp.HitObject as Slide).SlideInfoList.First().ID = ID;
                    EditorBeatmap.Update(bp.HitObject);
                }
                changeHandler.EndChange();
            };

            return new OsuMenuItem(SlidePaths.VALIDPATHS[ID].EndLane.ToString(), MenuItemType.Standard, commit);
        }
    }
}
