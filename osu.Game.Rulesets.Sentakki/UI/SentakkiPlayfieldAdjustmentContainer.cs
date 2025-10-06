using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI;

public partial class SentakkiPlayfieldAdjustmentContainer : PlayfieldAdjustmentContainer
{
    protected override Container<Drawable> Content => content;
    private readonly Container content;

    public SentakkiPlayfieldAdjustmentContainer()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        Size = new Vector2(.8f);

        InternalChild = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            FillMode = FillMode.Fit,
            FillAspectRatio = 1,
            Child = content = new ScalingContainer { RelativeSizeAxes = Axes.Both }
        };
    }

    /// <summary>
    /// A <see cref="Container"/> which scales its content relative to a target width.
    /// </summary>
    private partial class ScalingContainer : Container
    {
        protected override void Update()
        {
            base.Update();
            Scale = new Vector2(Parent!.ChildSize.X / SentakkiPlayfield.RINGSIZE);
            Size = Vector2.Divide(Vector2.One, Scale);
        }
    }
}
