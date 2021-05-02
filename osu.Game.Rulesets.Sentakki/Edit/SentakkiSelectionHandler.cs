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

        public SentakkiSelectionHandler()
        {
            selectionBreakState.ValueChanged += s =>
            {
                switch (s.NewValue)
                {
                    case TernaryState.False:
                        setBreakState(false);
                        break;

                    case TernaryState.True:
                        setBreakState(true);
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

        protected override void UpdateTernaryStates()
        {
            base.UpdateTernaryStates();

            selectionBreakState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<SentakkiLanedHitObject>(), h => h.Break);
        }

        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint<HitObject>> selection)
        {
            if (selection.Any(s => s.Item is SentakkiLanedHitObject))
                yield return new TernaryStateMenuItem("Break") { State = { BindTarget = selectionBreakState } };

            foreach (var item in base.GetContextMenuItemsForSelection(selection))
                yield return item;
        }
    }
}
