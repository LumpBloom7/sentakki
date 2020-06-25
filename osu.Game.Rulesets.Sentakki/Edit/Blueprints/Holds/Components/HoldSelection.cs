using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds.Components
{
    public class HoldSelection : BlueprintPiece<Hold>
    {
        private Container notebody;
        public HoldSelection()
        {
            Size = new Vector2(80);
            Position = Vector2.Zero;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            AddRangeInternal(new Drawable[]{
                notebody = new Container{
                    Position = new Vector2(0, -26),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.BottomCentre,
                    Size = new Vector2(80),
                    CornerRadius = Size.X / 2,
                    CornerExponent = 2,
                    Masking = true,
                    BorderColour = Colour4.White,
                    BorderThickness = 5,
                    Children = new Drawable[]{
                        new Box{
                            AlwaysPresent = true,
                            Alpha = 0,
                            RelativeSizeAxes = Axes.Both
                        },
                        new Container
                        {
                            Masking = true,
                            CornerExponent = 2.5f,
                            CornerRadius = 5f,
                            Rotation = 45,
                            Position = new Vector2(0, -40),
                            Size = new Vector2(20),
                            BorderColour = Color4.Gray,
                            BorderThickness = 2,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.Centre,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                AlwaysPresent = true,
                                Colour = Color4.White,
                            }
                        },
                        new Container
                        {
                            Masking = true,
                            CornerExponent = 2.5f,
                            CornerRadius = 5f,
                            Rotation = 45,
                            Position = new Vector2(0, 40),
                            Size = new Vector2(20),
                            BorderColour = Color4.Gray,
                            BorderThickness = 2,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                AlwaysPresent = true,
                                Colour = Color4.White,
                            }
                        }
                    },
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
        }

        public override void UpdateFrom(Hold hitObject)
        {
            //base.UpdateFrom(hitObject);
            Rotation = hitObject.Angle;
            notebody.Position = hitObject.EndPosition;
        }

        public void UpdateFrom(DrawableHold drawableHold)
        {
            notebody.Position = drawableHold.NoteBody.Position;
            notebody.Height = drawableHold.NoteBody.Height;
            Rotation = drawableHold.Rotation;
        }
    }
}