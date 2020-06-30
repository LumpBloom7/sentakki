using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Allocation;
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

            hitEvents = hitEvents.Where(e => !(e.HitObject is Hold)).ToList();
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Size = new Vector2(500, 150);
            AddRangeInternal(new Drawable[]{
                new NoteEntry
                {
                    ObjectName = "Tap",
                    HitEvents = hitEvents.Where(e=> e.HitObject is Tap && !(e.HitObject as SentakkiHitObject).IsBreak).ToList(),
                    Position = new Vector2(0, 0)
                },
                new NoteEntry
                {
                    ObjectName = "Hold",
                    HitEvents = hitEvents.Where(e=> (e.HitObject is HoldHead || e.HitObject is HoldTail) && !(e.HitObject as SentakkiHitObject).IsBreak).ToList(),
                    Position = new Vector2(0, .2f)
                },
                new NoteEntry
                {
                    ObjectName = "Touch",
                    HitEvents = hitEvents.Where(e=> e.HitObject is Touch).ToList(),
                    Position = new Vector2(0, .4f)
                },
                new NoteEntry
                {
                    ObjectName = "Touch Hold",
                    HitEvents = hitEvents.Where(e=> e.HitObject is TouchHold).ToList(),
                    Position = new Vector2(0, .6f)
                },
                new NoteEntry
                {
                    ObjectName = "Break",
                    HitEvents = hitEvents.Where(e=> (e.HitObject as SentakkiHitObject).IsBreak).ToList(),
                    Position = new Vector2(0, .8f)
                },
            });
        }
        public class NoteEntry : Container
        {
            private Container progressBox;

            public string ObjectName = "Object";
            public List<HitEvent> HitEvents;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                float MehCount = 0;
                float GoodCount = 0;
                float GreatCount = 0;

                foreach (var e in HitEvents)
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

                Color4 textColour = HitEvents.Count == 0 ? Color4Extensions.FromHex("bcbcbc") : (GreatCount == HitEvents.Count) ? Color4.White : Color4Extensions.FromHex("#3c5394");
                Color4 boxColour = HitEvents.Count == 0 ? Color4Extensions.FromHex("808080") : (GreatCount == HitEvents.Count) ? Color4Extensions.FromHex("fda908") : Color4Extensions.FromHex("#DCE9F9");
                Color4 borderColour = HitEvents.Count == 0 ? Color4Extensions.FromHex("536277") : (GreatCount == HitEvents.Count) ? Color4Extensions.FromHex("fda908") : Color4Extensions.FromHex("#98b8df");
                Color4 numberColour = (GreatCount == HitEvents.Count && HitEvents.Count > 0) ? Color4.White : Color4Extensions.FromHex("#3c5394");

                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
                RelativePositionAxes = Axes.Both;
                RelativeSizeAxes = Axes.Both;
                Size = new Vector2(1, .2f);
                Masking = true;
                BorderThickness = 2;
                BorderColour = borderColour;
                CornerRadius = 5;
                CornerExponent = 2.5f;

                InternalChildren = new Drawable[]{
                    new Box{
                        RelativeSizeAxes = Axes.Both,
                        Colour = boxColour,
                    },
                    new Container{ // Left
                        RelativeSizeAxes = Axes.Both,
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                        Size = new Vector2(.33f, 1),
                        Child = new OsuSpriteText
                        {
                            Colour = textColour,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = ObjectName.ToUpper(),
                            Font = OsuFont.Torus.With(size: 20, weight: FontWeight.Bold)
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
                                Colour = (HitEvents.Count ==0) ? Color4Extensions.FromHex("343434"):Color4.DarkGray,
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
                            Colour = numberColour,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = HitEvents.Count.ToString(),
                            Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold)
                        }
                    },
                };

                progressBox.AddRange(new Drawable[]{
                    new ProgressBox(HitResult.Meh, MehCount/HitEvents.Count),
                    new ProgressBox(HitResult.Good, GoodCount/HitEvents.Count),
                    new ProgressBox(HitResult.Great, GreatCount/HitEvents.Count)
                });
            }
            private class ProgressBox : Container
            {
                private HitResult result;
                public ProgressBox(HitResult result, float progress)
                {
                    this.result = result;
                    RelativeSizeAxes = Axes.Both;
                    Size = new Vector2(float.IsNaN(progress) ? 0 : progress, 1);
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
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