using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace osu.Game.Rulesets.Sentakki.Statistics
{
    internal class ChartEntry : CompositeDrawable
    {
        private static readonly Color4 accent_color = Color4Extensions.FromHex("#66FFCC");

        private static readonly Color4 background_color = Color4Extensions.FromHex("#202624");

        private const double bar_fill_duration = 3000;

        private readonly string name;

        private readonly IReadOnlyList<HitEvent> hitEvents;

        private RollingCounter<int> noteCounter;

        private Container ratioBoxes;

        public ChartEntry(string name, IReadOnlyList<HitEvent> hitEvents)
        {
            this.name = name;
            this.hitEvents = hitEvents;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Origin = Anchor.TopCentre;
            RelativeSizeAxes = Axes.Both;
            Height = 1f / 6f;
            Scale = new Vector2(1, 0);

            Masking = true;
            BorderThickness = 2;
            BorderColour = accent_color;
            CornerRadius = 5;
            CornerExponent = 2.5f;

            Alpha = hitEvents.Any() ? 1 : 0.8f;
            Colour = !hitEvents.Any() ? Color4.DarkGray : Color4.White;

            bool allPerfect = hitEvents.Any() && hitEvents.All(h => h.Result == HitResult.Great);

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = background_color,
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Content = new[]{
                        new Drawable[]{
                            new OsuSpriteText
                            {
                                Colour = accent_color,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = name.ToUpper(),
                                Font = OsuFont.Torus.With(size: 20, weight: FontWeight.Bold)
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Origin = Anchor.Centre,
                                Anchor = Anchor.Centre,
                                CornerRadius = 5,
                                CornerExponent = 2.5f,
                                Masking = true,
                                BorderThickness = 2,
                                BorderColour = accent_color,
                                Height = 0.8f,
                                Children = new Drawable[]{
                                    new Box
                                    {
                                        Alpha = 0,
                                        AlwaysPresent = true,
                                        RelativeSizeAxes = Axes.Both
                                    },
                                    ratioBoxes = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Scale = new Vector2(0,1),
                                    }
                                }
                            },
                            noteCounter = new TotalNoteCounter
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Colour = accent_color,
                                Current = { Value = 0 },
                            }
                        }
                    }
                },
            };

            if (!hitEvents.Any()) return;

            addRatioBoxFor(HitResult.Ok);
            addRatioBoxFor(HitResult.Good);
            addRatioBoxFor(HitResult.Great);
        }

        public void AnimateEntry(double entryDuration)
        {
            this.ScaleTo(1, entryDuration, Easing.OutBack).TransformBindableTo(noteCounter.Current, hitEvents.Count);
            ratioBoxes.ScaleTo(1, bar_fill_duration, Easing.OutPow10);
        }


        // This will add a box for each valid sentakki HitResult, excluding those that aren't visible
        private void addRatioBoxFor(HitResult result)
        {
            int resultCount = hitEvents.Count(e => e.Result >= result);

            if (resultCount == 0) return;

            ratioBoxes.Add(new RatioBox
            {
                Width = (float)resultCount / hitEvents.Count,
                Colour = result.GetColorForSentakkiResult(),
            });
        }

        public class TotalNoteCounter : RollingCounter<int>
        {
            protected override double RollingDuration => bar_fill_duration;

            protected override Easing RollingEasing => Easing.OutPow10;

            protected override string FormatCount(int count) => count.ToString("N0");

            protected override OsuSpriteText CreateSpriteText() => new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold),
            };
        }

        private class RatioBox : Sprite
        {
            // Replace this shit with a shader ASAP
            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                // Using a publically accessible texture
                Texture = textures.Get("Icons/BeatmapDetails/accuracy", WrapMode.Repeat, WrapMode.Repeat);
                Texture.SetData(generateTexture());
                RelativeSizeAxes = Axes.Both;
                TextureRelativeSizeAxes = Axes.None;
                TextureRectangle = new Framework.Graphics.Primitives.RectangleF(0, 0, 50, 50);
            }

            private TextureUpload generateTexture()
            {
                const int line_thickness = 4;
                const int line_spacing = 6;
                const int line_distance = line_thickness + line_spacing;
                Image<Rgba32> image = new Image<Rgba32>(50, 50);
                for (int y = 0; y < 50; ++y)
                {
                    var span = image.GetPixelRowSpan(y);
                    for (int x = 0; x < 50; ++x)
                    {
                        bool pixelLit = ((x + y) % line_distance) <= line_thickness;
                        span[x] = new Rgba32(1f, 1f, 1f, pixelLit ? 1f : 0f);
                    }
                }
                return new TextureUpload(image);
            }
        }
    }
}
