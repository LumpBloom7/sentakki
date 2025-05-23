using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Touches;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds
{
    public partial class TouchHoldCentrePiece : CompositeDrawable
    {
        public Container PieceContainer;

        [Resolved]
        private Bindable<IReadOnlyList<Color4>>? paletteBindable { get; set; } = null!;

        private Container progressParts;

        public TouchHoldCentrePiece()
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Rotation = 45;
            Scale = new Vector2(80 / 90f);

            InternalChildren =
            [
                PieceContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Children =
                    [
                        createTouchShapeWith<TouchPieceShadow>(),
                        progressParts = createTouchShapeWith<TouchHoldPiece>(),
                    ]
                },
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            paletteBindable?.BindValueChanged(p =>
            {
                for (int i = 0; i < progressParts.Count; ++i)
                    progressParts[i].Colour = p.NewValue[i];
            }, true);
        }

        // Creates the touch shape using the provided drawable as each of the 4 quarters
        private Container createTouchShapeWith<T>() where T : Drawable, new()
            => new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Children = [
                    new T
                    {
                        Anchor = Anchor.TopCentre,
                    },
                    new T
                    {
                        Anchor = Anchor.CentreRight,
                        Rotation = 90,
                    },
                    new T
                    {
                        Anchor = Anchor.BottomCentre,
                        Rotation = 180,
                    },
                    new T
                    {
                        Anchor = Anchor.CentreLeft,
                        Rotation = 270,
                    },
                ]
            };
    }
}
