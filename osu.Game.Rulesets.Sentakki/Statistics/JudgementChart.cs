using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Scoring;
using osuTK;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Ranking.Statistics;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Mods;
using osu.Game.Rulesets.Sentakki.Replays;
using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Statistics
{
    public class JudgementChart : CompositeDrawable
    {
        public JudgementChart(List<HitEvent> hitEvents)
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Size = new Vector2(250, 150);
            AddRangeInternal(new Drawable[]{
                new NoteEntry<Tap>(hitEvents){Position = new Vector2(0, 0)},
                new NoteEntry<Hold>(hitEvents){Position = new Vector2(0, .2f)},
                new NoteEntry<Touch>(hitEvents){Position = new Vector2(0, .4f)},
                new NoteEntry<TouchHold>(hitEvents){Position = new Vector2(0, .6f)},
            });
        }
        public class NoteEntry<T> : Container where T : SentakkiHitObject, new()
        {
            private Container progressBox;
            private List<HitEvent> events;
            public NoteEntry(List<HitEvent> hitEvents)
            {
                T tmp = new T();
                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
                events = hitEvents.Where(e => e.HitObject.GetType() == tmp.GetType() && !(e.HitObject as SentakkiHitObject).IsBreak).ToList();
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
                            Colour = Color4.Blue,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = typeof(T).Name.ToUpper(),
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
                    new Container{ // Right
                        RelativeSizeAxes = Axes.Both,
                        Origin = Anchor.CentreRight,
                        Anchor = Anchor.CentreRight,
                        Size = new Vector2(.33f, 1),
                        Child = new OsuSpriteText
                        {
                            Colour = Color4.Blue,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = events.Count.ToString(),
                            Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold)
                        }
                    },
                };

                float MehCount = 0;
                float GoodCount = 0;
                float GreatCount = 0;

                foreach (var e in events)
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
                    new ProgressBox(HitResult.Meh, MehCount/events.Count),
                    new ProgressBox(HitResult.Good, GoodCount/events.Count),
                    new ProgressBox(HitResult.Great, GreatCount/events.Count)
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