using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sentakki.UI;

namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class DrawableSentakkiEditorRuleset : DrawableSentakkiRuleset
{
    public DrawableSentakkiEditorRuleset(SentakkiRuleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod>? mods) : base(ruleset, beatmap, mods) { }

    public BindableBool ShowSpeedChanges { get; } = new BindableBool();

    public double TimelineTimeRange { get; set; }

    [Resolved]
    private GameHost gameHost { get; set; } = null!;

    protected override void Update()
    {
        if (ShowSpeedChanges.Value)
            base.Update();
        else
        {
            AdjustedAnimDuration.Value = Interpolation.DampContinuously(AdjustedAnimDuration.Value, TimelineTimeRange, 50, gameHost.UpdateThread.Clock.ElapsedFrameTime);
            AdjustedTouchAnimDuration.Value = Interpolation.DampContinuously(AdjustedTouchAnimDuration.Value, TimelineTimeRange, 50, gameHost.UpdateThread.Clock.ElapsedFrameTime);
        }
    }
}
