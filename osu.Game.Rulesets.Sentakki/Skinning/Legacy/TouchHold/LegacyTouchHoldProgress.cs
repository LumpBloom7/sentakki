using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy.TouchHold;

public partial class LegacyTouchHoldProgress : CompositeDrawable
{
    public readonly Bindable<float> ProgressBindable = new();

    private LegacyTouchHoldProgressQuadrant[] progressQuadrants = new LegacyTouchHoldProgressQuadrant[4];

    [Resolved]
    private Bindable<IReadOnlyList<Color4>>? paletteBindable { get; set; }

    public LegacyTouchHoldProgress()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
    }

    [BackgroundDependencyLoader]
    private void load(ISkinSource skin)
    {
        var texture = skin.GetTexture("sentakki/touchhold-progress");

        if (texture is null)
            return;

        InternalChildren = progressQuadrants = [
            new LegacyTouchHoldProgressQuadrant{ Origin = Anchor.BottomLeft },
            new LegacyTouchHoldProgressQuadrant{ Origin = Anchor.TopLeft },
            new LegacyTouchHoldProgressQuadrant{ Origin = Anchor.TopRight },
            new LegacyTouchHoldProgressQuadrant{ Origin = Anchor.BottomRight }
        ];

        ProgressBindable.BindValueChanged(v =>
        {
            foreach (var quadrant in progressQuadrants)
                quadrant.Progress = v.NewValue;
        }, true);

        paletteBindable?.BindValueChanged(c =>
        {
            for (int i = 0; i < 4; ++i)
                progressQuadrants[i].AccentColour = c.NewValue[i];
        });
    }
}
