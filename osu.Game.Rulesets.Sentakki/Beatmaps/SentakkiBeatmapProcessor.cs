using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Beatmaps
{
    public class SentakkiBeatmapProcessor : BeatmapProcessor
    {
        public SentakkiBeatmapProcessor(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        public override void PreProcess()
        {
            base.PreProcess();

            var senBeatmap = (Beatmap<SentakkiHitObject>)Beatmap;
            foreach (var HitObject in senBeatmap.HitObjects)
            {
                HitObject.Lane = HitObject.Lane.NormalizePath();
            }
            mergeSlides(senBeatmap);
        }

        private void mergeSlides(Beatmap<SentakkiHitObject> senBeatmap)
        {
            Dictionary<double, Dictionary<int, List<Slide>>> slides = new Dictionary<double, Dictionary<int, List<Slide>>>();

            foreach (Slide slide in senBeatmap.HitObjects.Where(obj => obj is Slide).ToList())
            {
                if (!slides.ContainsKey(slide.StartTime))
                    slides.Add(slide.StartTime, new Dictionary<int, List<Slide>>());

                if (!slides[slide.StartTime].ContainsKey(slide.Lane))
                    slides[slide.StartTime].Add(slide.Lane, new List<Slide>());

                slides[slide.StartTime][slide.Lane].Add(slide);
                senBeatmap.HitObjects.Remove(slide);
            }

            foreach (var StartTime in slides)
            {
                foreach (var lane in StartTime.Value)
                {
                    Slide newSlide = null;
                    foreach (var slide in lane.Value)
                    {
                        if (newSlide == null)
                            newSlide = new Slide
                            {
                                StartTime = slide.StartTime,
                                EndTime = slide.EndTime,
                                Samples = slide.Samples,
                                IsBreak = slide.IsBreak,
                                Lane = slide.Lane,
                                SlidePathIDs = new List<int>(),
                                HasTwin = slide.HasTwin,
                            };

                        newSlide.SlidePathIDs.AddRange(slide.SlidePathIDs);
                    }
                    if (newSlide != null)
                        senBeatmap.HitObjects.Add(newSlide);
                }
            }
            senBeatmap.HitObjects.Sort((lhs, rhs) => lhs.StartTime.CompareTo(rhs.StartTime));
        }
    }
}