using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK;

using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Statistics
{
    public class JudgementChart : CompositeDrawable
    {
        public JudgementChart(List<HitEvent> hitEvents)
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Size = new Vector2(500, 150);
            AddRangeInternal(new Drawable[]{
                new NoteEntry("Tap",hitEvents.Where(e=> e.HitObject is Tap && !(e.HitObject as SentakkiHitObject).IsBreak).ToList()){
                    Position = new Vector2(0, 0)
                },
                new NoteEntry("Hold",hitEvents.Where(e=> e.HitObject is Hold && !(e.HitObject as SentakkiHitObject).IsBreak).ToList()){
                    Position = new Vector2(0, .2f)
                },
                new NoteEntry("Touch",hitEvents.Where(e=> e.HitObject is Touch).ToList()){
                    Position = new Vector2(0, .4f)
                },
                new NoteEntry("Touch Hold",hitEvents.Where(e=> e.HitObject is TouchHold).ToList()){
                    Position = new Vector2(0, .6f)
                },
                new NoteEntry("Break",hitEvents.Where(e=> (e.HitObject as SentakkiHitObject).IsBreak).ToList()){
                    Position = new Vector2(0, .8f)
                },
            });
        }
        public class NoteEntry : Container
        {
            private Container progressBox;
            public NoteEntry(string objectName, List<HitEvent> hitEvents)
            {
                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
                RelativePositionAxes = Axes.Both;
                RelativeSizeAxes = Axes.Both;
                Size = new Vector2(1, .2f);
                Masking = true;
                BorderThickness = 2;
                BorderColour = Color4.Blue;
                CornerRadius = 5;
                CornerExponent = 2.5f;

                InternalChildren = new Drawable[]{
                    new Box{
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.LightBlue,
                    },
                    new Container{ // Left
                        RelativeSizeAxes = Axes.Both,
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                        Size = new Vector2(.33f, 1),
                        Child = new OsuSpriteText
                        {
                            Colour = Color4Extensions.FromHex("#435ca6"),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = objectName.ToUpper(),
                            Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold)
                        }
        },
                    progressBox = new Container{ // Centre
                        RelativeSizeAxes = Axes.Both,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Size = new Vector2(.34f, .80f),
                        Masking = true,
                        BorderThickness = 2,
                        BorderColour = Color4.Black,
                        Children = new Drawable[]{
                            new Box{
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.DarkGray,
                            }
}
                    },
                    new Container
                    { // Right
                        RelativeSizeAxes = Axes.Both,
                        Origin = Anchor.CentreRight,
                        Anchor = Anchor.CentreRight,
                        Size = new Vector2(.33f, 1),
                        Child = new OsuSpriteText
                        {
                            Colour = Color4.Blue,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = hitEvents.Count.ToString(),
                            Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold)
                        }
                    },
                };

                float MehCount = 0;
                float GoodCount = 0;
                float GreatCount = 0;

                foreach (var e in hitEvents)
                {
                    switch (e.Result)
                    {
                        case HitResult.Perfect:
                        case HitResult.Great:
                            ++GreatCount;
                            goto case HitResult.Good;
                        case HitResult.Good:
                            ++GoodCount;
                            goto case HitResult.Meh;
                        case HitResult.Meh:
                            ++MehCount;
                            break;
                    }
                }

                progressBox.AddRange(new Drawable[]{
                    new ProgressBox(HitResult.Meh, MehCount/hitEvents.Count),
                    new ProgressBox(HitResult.Good, GoodCount/hitEvents.Count),
                    new ProgressBox(HitResult.Great, GreatCount/hitEvents.Count)
                });
            }
            private class ProgressBox : Container
            {
                private OsuColour colours = new OsuColour();
                public ProgressBox(HitResult result, float progress)
                {
                    RelativeSizeAxes = Axes.Both;
                    Size = new Vector2(float.IsNaN(progress) ? 0 : progress, 1);
                    Add(new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.ForHitResult(result)
                    });
                }
            }
        }
    }
}