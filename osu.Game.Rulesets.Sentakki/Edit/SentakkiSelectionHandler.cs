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

        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint> selection)
        {
            if (selection.All(s => s.HitObject is SentakkiLanedHitObject))
                yield return new TernaryStateMenuItem("Break") { State = { BindTarget = selectionBreakState } };
        }
        protected override void UpdateTernaryStates()
        {
            base.UpdateTernaryStates();

            selectionBreakState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<SentakkiLanedHitObject>(), h => h.Break);
        }
    }
}
