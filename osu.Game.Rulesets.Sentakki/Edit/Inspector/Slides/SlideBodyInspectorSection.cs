using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;

namespace osu.Game.Rulesets.Sentakki.Edit.Inspector.Slides;

public partial class SlideBodyInspectorSection : CompositeDrawable
{
    protected FillFlowContainer segmentEntries = null!;

    private IBindable<int> slideBodyVersion = new Bindable<int>();

    private Slide slide;
    private SlideBodyInfo slideBodyInfo;

    public SlideBodyInspectorSection(Slide slide, SlideBodyInfo slideBodyInfo)
    {
        this.slide = slide;
        this.slideBodyInfo = slideBodyInfo;

        AutoSizeAxes = Axes.Y;
        RelativeSizeAxes = Axes.X;

        InternalChild = segmentEntries = new FillFlowContainer
        {
            AutoSizeAxes = Axes.Y,
            RelativeSizeAxes = Axes.X,
            Direction = FillDirection.Vertical
        };

        slideBodyVersion.BindTo(slideBodyInfo.Version);
        slideBodyVersion.BindValueChanged(_ => populateEntries(), true);
    }

    private void populateEntries()
    {
        // The entries will update themselves when SlideBodyInfo changes
        // This prevents the popup from closing automatically
        // We only need to remove excess entries or add missing ones to match the current count.

        if (segmentEntries.Count >= slideBodyInfo.Segments.Count)
        {
            segmentEntries.RemoveRange([.. segmentEntries.Children.Skip(slideBodyInfo.Segments.Count)], true);
            return;
        }

        for (int i = segmentEntries.Count; i < slideBodyInfo.Segments.Count; ++i)
            segmentEntries.Add(new SentakkiSlideSegmentInspectorEntry(slide, slideBodyInfo, i));
    }
}
