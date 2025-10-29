using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Snapping;

public partial class SentakkiSnapProvider : CompositeDrawable
{
    public enum SnapMode
    {
        Off,
        Laned,
        Touch,
    }

    private SnapMode activeMode = SnapMode.Off;

    private readonly SentakkiLanedSnapGrid lanedSnapGrid;
    public readonly SentakkiTouchSnapGrid TouchSnapGrid;

    public IEnumerable<DrawableTernaryButton> CreateTernaryButtons()
    {
        yield return lanedSnapGrid.CreateTernaryButton();
        yield return TouchSnapGrid.CreateTernaryButton();
    }

    public SentakkiSnapProvider()
    {
        Anchor = Origin = Anchor.Centre;

        AddRangeInternal([
            lanedSnapGrid = new SentakkiLanedSnapGrid(),
            TouchSnapGrid = new SentakkiTouchSnapGrid()
        ]);

        SwitchModes(SnapMode.Off);
    }

    public SnapResult GetSnapResult(Vector2 screenSpacePosition)
    {
        return activeMode switch
        {
            SnapMode.Laned => lanedSnapGrid.GetSnapResult(screenSpacePosition),
            SnapMode.Touch => TouchSnapGrid.GetSnapResult(screenSpacePosition),
            _ => new SnapResult(screenSpacePosition, null),
        };
    }

    public void SwitchModes(SnapMode mode)
    {
        activeMode = mode;

        switch (mode)
        {
            case SnapMode.Off:
                lanedSnapGrid.Hide();
                TouchSnapGrid.Hide();
                break;

            case SnapMode.Laned:
                lanedSnapGrid.Show();
                TouchSnapGrid.Hide();
                break;

            case SnapMode.Touch:
                lanedSnapGrid.Hide();
                TouchSnapGrid.Show();
                break;
        }
    }

    public float GetDistanceRelativeToCurrentTime(double time, float min = float.MinValue, float max = float.MaxValue) => lanedSnapGrid.GetDistanceRelativeToCurrentTime(time, min, max);

    public double GetDistanceBasedSnapTime(Vector2 screenSpacePosition) => lanedSnapGrid.SnappedDistanceTime(screenSpacePosition);
}
