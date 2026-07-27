using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Utils;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy.TouchHold;

public partial class LegacyTouchHoldProgressQuadrant : CompositeDrawable
{
    private CircularProgress shadowPiece = null!;
    private CircularProgress progressPiece = null!;

    public double Progress
    {
        get => progressPiece.Progress;
        set
        {
            shadowPiece.Progress = value;
            progressPiece.Progress = value;
        }
    }

    public LegacyTouchHoldProgressQuadrant()
    {
        RelativeSizeAxes = Axes.Both;
        Size = new Vector2(1f);
        Anchor = Anchor.Centre;
        Masking = true;
        MaskingSmoothness = 0;
    }

    [BackgroundDependencyLoader]
    private void load(ISkinSource skin)
    {
        var texture = skin.GetTexture("sentakki/touchhold-progress");
        var glowTexture = skin.GetTexture("sentakki/glow/touchhold-progress");

        AddInternal(shadowPiece = new CircularProgress
        {
            Anchor = Origin,
            Origin = Anchor.Centre,

            RelativeSizeAxes = Axes.Both,
            Scale = new Vector2(1.5f),
            Texture = glowTexture,
            Colour = Color4.Black,
        });

        AddInternal(progressPiece = new CircularProgress
        {
            Anchor = Origin,
            Origin = Anchor.Centre,

            RelativeSizeAxes = Axes.Both,
            Texture = texture
        });
    }

    [Resolved]
    private Bindable<bool>? isHitting { get; set; }

    private Color4 originalColour;
    private Color4 flashingColour;

    public Color4 AccentColour
    {
        get => originalColour;
        set
        {
            originalColour = value;
            flashingColour = value.LightenHsl(0.4f);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (isHitting?.Value ?? false)
        {
            const double flashing_time = 80;

            double flashProg = Time.Current % (flashing_time * 2) / (flashing_time * 2);

            if (flashProg <= 0.5)
                Colour = Interpolation.ValueAt(flashProg, originalColour, flashingColour, 0, 0.5, Easing.OutSine);
            else
                Colour = Interpolation.ValueAt(flashProg, flashingColour, originalColour, 0.5, 0, Easing.InSine);
        }
        else
        {
            Colour = originalColour;
        }
    }
}
