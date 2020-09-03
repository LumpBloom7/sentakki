using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Framework.Graphics.Lines;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides.Components
{
    public class SlideEditBody : BlueprintPiece<Slide>
    {
        private readonly SlideEditPath body;

        public SlideEditBody()
        {
            InternalChild = body = new SlideEditPath();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
        }

        public override void UpdateFrom(Slide hitObject)
        {
            Rotation = hitObject.Lane.GetRotationForLane() - 22.5f;

            var vertices = new List<Vector2>();
            hitObject.SlideInfoList.First().SlidePath.Path.GetPathToProgress(vertices, 0, 1);
            body.Vertices = vertices;
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => body.ReceivePositionalInputAt(screenSpacePos);

        public class SlideEditPath : SmoothPath
        {
            public override Vector2 OriginPosition => Vertices.Any() ? PositionInBoundingBox(Vertices[0]) : base.OriginPosition;

            public SlideEditPath()
            {
                PathRadius = 23;
                Position = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0);
            }

            protected override Color4 ColourAt(float position) => position <= .2f ? Color4.White : Color4.Transparent;
        }
    }
}
