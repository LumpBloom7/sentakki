
using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Utils;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;

public partial class SlideSelectionBlueprint : SentakkiSelectionBlueprint<Slide, DrawableSlide>
{
    private readonly SlideBodyHighlight? slideBodyHighlight;
    private readonly SlideTapPiece slideTapHighlight;
    private readonly TapPiece tapHighlight;

    public override Quad SelectionQuad => slideTapHighlight.ScreenSpaceDrawQuad;
    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        => slideTapHighlight.ReceivePositionalInputAt(screenSpacePos) || DrawableObject.SlideBodies.Select(s => s.Slidepath).Any(s => s.ReceivePositionalInputAt(screenSpacePos));

    public SlideSelectionBlueprint(Slide item)
        : base(item)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        if (item.SlideInfoList.Count == 1)
            AddInternal(slideBodyHighlight = new SlideBodyHighlight(item, item.SlideInfoList[0]) { });

        AddInternal(
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Colour = Color4.YellowGreen,
                Children = [
                    slideTapHighlight = new SlideTapPiece(),
                    tapHighlight = new TapPiece()
                ]
            }
        );
    }

    [Resolved]
    private EditorClock editorClock { get; set; } = null!;

    private readonly Bindable<double> animationSpeed = new Bindable<double>(5);

    [BackgroundDependencyLoader]
    private void load(SentakkiBlueprintContainer blueprintContainer)
    {
        animationSpeed.BindTo(blueprintContainer.Composer.DrawableRuleset.AdjustedAnimDuration);
    }

    protected override void Update()
    {
        base.Update();

        Rotation = HitObject.Lane.GetRotationForLane();

        slideTapHighlight.Scale = new Vector2(Math.Clamp(
            Interpolation.ValueAt(HitObject.StartTime, 1f, 0f, editorClock.CurrentTime + (animationSpeed.Value / 2), editorClock.CurrentTime + animationSpeed.Value),
            0f, 1f));

        tapHighlight.Scale = slideTapHighlight.Scale;

        slideTapHighlight.Y = -Interpolation.ValueAt(
                HitObject.StartTime,
                SentakkiPlayfield.INTERSECTDISTANCE,
                SentakkiPlayfield.NOTESTARTDISTANCE,
                editorClock.CurrentTime,
                editorClock.CurrentTime + (animationSpeed.Value / 2)
            );

        slideTapHighlight.Y = Math.Clamp(slideTapHighlight.Y, -SentakkiPlayfield.INTERSECTDISTANCE, -SentakkiPlayfield.NOTESTARTDISTANCE);
        tapHighlight.Y = slideTapHighlight.Y;

        if (Item.TapType is Slide.TapTypeEnum.None)
        {
            slideTapHighlight.Alpha = 0.5f;
            tapHighlight.Alpha = 0;
        }
        else if (Item.TapType is Slide.TapTypeEnum.Tap)
        {
            slideTapHighlight.Alpha = 0f;
            tapHighlight.Alpha = 1;
        }
        else
        {
            SlideTapPiece tapVisual = (SlideTapPiece)DrawableObject.SlideTaps.Child.TapVisual;

            slideTapHighlight.Stars.Rotation = tapVisual.Stars.Rotation;
            slideTapHighlight.SecondStar.Alpha = tapVisual.SecondStar.Alpha;

            slideTapHighlight.Alpha = 1f;
            tapHighlight.Alpha = 0;
        }

        if (slideBodyHighlight is null)
            return;

        slideBodyHighlight.UpdateFrom(DrawableObject.SlideBodies[0]);
    }
}
