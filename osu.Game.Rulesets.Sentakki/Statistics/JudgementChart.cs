using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK;
using osuTK.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace osu.Game.Rulesets.Sentakki.Statistics
{
    public class JudgementChart : FillFlowContainer
    {
        private const double entry_animation_duration = 150;
        private const double bar_fill_duration = 3000;

        public JudgementChart(List<HitEvent> hitEvents)
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Size = new Vector2(500, 250);
            //RelativeSizeAxes = Axes.Both;

            AddRangeInternal(new Drawable[]{
                new NoteEntry(0)
                {
                    ObjectName = "Tap",
                    HitEvents = hitEvents.Where(e=> e.HitObject is Tap x && !x.Break).ToList(),
                },
                new NoteEntry(1)
                {
                    ObjectName = "Hold",
                    HitEvents = hitEvents.Where(e => (e.HitObject is Hold x || e.HitObject is Hold.HoldHead) && !(e.HitObject as SentakkiLanedHitObject).Break).ToList(),
                },
                new NoteEntry(2)
                {
                    ObjectName = "Slide",
                    HitEvents = hitEvents.Where(e => e.HitObject is SlideBody).ToList(),
                },
                new NoteEntry(3)
                {
                    ObjectName = "Touch",
                    HitEvents = hitEvents.Where(e => e.HitObject is Touch).ToList(),
                },
                new NoteEntry(4)
                {
                    ObjectName = "Touch Hold",
                    HitEvents = hitEvents.Where(e => e.HitObject is TouchHold).ToList(),
                },
                new NoteEntry(5)
                {
                    ObjectName = "Break",
                    HitEvents = hitEvents.Where(e => e.HitObject is SentakkiLanedHitObject x && x.Break).ToList(),
                },
            });
        }

        private class NoteEntry : CompositeDrawable
        {
            public NoteEntry(int entryIndex)
            {
                AlwaysPresent = true;
                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
                RelativeSizeAxes = Axes.Both;
                Scale = new Vector2(1, 0);

                Masking = true;
                BorderThickness = 2;
                CornerRadius = 5;
                CornerExponent = 2.5f;

                Height = 1f / 6f;

                InitialLifetimeOffset = entry_animation_duration * entryIndex;
            }

            public double InitialLifetimeOffset;
            private RollingCounter<long> noteCounter;

            private JudgementRatioBox judgementRatio;

            public string ObjectName = "Object";
            public List<HitEvent> HitEvents;

            [BackgroundDependencyLoader]
            private void load()
            {
                Alpha = HitEvents.Any() ? 1 : 0.8f;
                Colour = !HitEvents.Any() ? Color4.DarkGray : Color4.White;
                bool allPerfect = HitEvents.Any() && HitEvents.All(h => h.Result == HitResult.Great);

                Color4 textColour = Color4Extensions.FromHex("#66FFCC");
                Color4 boxColour = Color4Extensions.FromHex("#202624");
                Color4 borderColour = Color4Extensions.FromHex("#66FFCC");
                Color4 numberColour = Color4Extensions.FromHex("#66FFCC");

                BorderColour = borderColour;

                InternalChildren = new Drawable[]{
                    new Box {
                        RelativeSizeAxes = Axes.Both,
                        Colour = boxColour,
                    },
                    new GridContainer{
                        RelativeSizeAxes = Axes.Both,
                        Content = new[]{
                            new Drawable[]{
                                new OsuSpriteText
                                {
                                    Colour = textColour,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = ObjectName.ToUpper(),
                                    Font = OsuFont.Torus.With(size: 20, weight: FontWeight.Bold)
                                },
                                judgementRatio = new JudgementRatioBox(HitEvents),
                                noteCounter = new TotalNoteCounter
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Colour = numberColour,
                                    Current = { Value = 0 },
                                }
                            }
                        }
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                ScheduleAfterChildren(() =>
                {
                    using (BeginDelayedSequence(InitialLifetimeOffset, true))
                    {
                        this.ScaleTo(1, entry_animation_duration, Easing.OutBack);
                        noteCounter.Current.Value = HitEvents.Count;
                        judgementRatio.AnimateEntry();
                    }
                });
            }

            public class TotalNoteCounter : RollingCounter<long>
            {
                protected override double RollingDuration => bar_fill_duration;

                protected override Easing RollingEasing => Easing.OutPow10;

                protected override string FormatCount(long count) => count.ToString("N0");

                protected override OsuSpriteText CreateSpriteText()
                {
                    return new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold),
                    };
                }
            }

            private class JudgementRatioBox : CompositeDrawable
            {
                private readonly Container ratioBoxes;

                public JudgementRatioBox(List<HitEvent> hitEvents)
                {
                    RelativeSizeAxes = Axes.Both;
                    Origin = Anchor.Centre;
                    Anchor = Anchor.Centre;
                    CornerRadius = 5;
                    CornerExponent = 2.5f;
                    Masking = true;
                    BorderThickness = 2;
                    BorderColour = Color4Extensions.FromHex("#66FFCC");
                    Height = 0.8f;

                    InternalChildren = new Drawable[]{
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
                    };

                    if (hitEvents.Any())
                    {
                        void addRatioBox(HitResult result)
                        {
                            int resultCount = hitEvents.Count(h => h.Result >= result);
                            if (resultCount == 0) return;

                            ratioBoxes.Add(new RatioBox
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = (float)resultCount / hitEvents.Count,
                                Colour = result.GetColorForSentakkiResult(),
                                Alpha = 0.8f
                            });
                        };

                        addRatioBox(HitResult.Ok);
                        addRatioBox(HitResult.Good);
                        addRatioBox(HitResult.Great);
                    }
                }

                public void AnimateEntry() => ratioBoxes.ScaleTo(1, bar_fill_duration, Easing.OutPow10);

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
    }
}
