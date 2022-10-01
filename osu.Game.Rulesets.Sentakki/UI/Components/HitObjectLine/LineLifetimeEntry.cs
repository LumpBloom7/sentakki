using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Performance;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI.Components.HitObjectLine
{
    public class LineLifetimeEntry : LifetimeEntry
    {
        public BindableDouble AnimationDuration = new BindableDouble(1000);
        public double AdjustedAnimationDuration => AnimationDuration.Value * GameplaySpeed;

        public double GameplaySpeed => drawableRuleset?.GameplaySpeed ?? 1;

        private readonly DrawableSentakkiRuleset? drawableRuleset;

        public double StartTime { get; private set; }

        public LineLifetimeEntry(BindableDouble AnimationDuration, DrawableSentakkiRuleset? drawableSentakkiRuleset, double startTime)
        {
            StartTime = startTime;
            drawableRuleset = drawableSentakkiRuleset;
            this.AnimationDuration.BindTo(AnimationDuration);
            this.AnimationDuration.BindValueChanged(refreshLifetime, true);
        }

        public List<SentakkiLanedHitObject> HitObjects = new List<SentakkiLanedHitObject>();

        public LineType Type { get; private set; }
        public ColourInfo Colour { get; private set; }
        public float Rotation { get; private set; }

        public void Add(SentakkiLanedHitObject hitObject)
        {
            hitObject.LaneBindable.ValueChanged += onLaneChanged;
            hitObject.BreakBindable.ValueChanged += onBreakChanged;
            HitObjects.AddInPlace(hitObject, Comparer<SentakkiLanedHitObject>.Create((lhs, rhs) => lhs.Lane.CompareTo(rhs.Lane)));
            UpdateLine();
        }

        public void Remove(SentakkiLanedHitObject hitObject)
        {
            hitObject.LaneBindable.ValueChanged -= onLaneChanged;
            hitObject.BreakBindable.ValueChanged -= onBreakChanged;
            HitObjects.Remove(hitObject);
            UpdateLine();
        }

        private void onLaneChanged(ValueChangedEvent<int> obj) => UpdateLine();

        private void onBreakChanged(ValueChangedEvent<bool> obj) => UpdateLine();

        public Action<LineLifetimeEntry> OnLineUpdated = null!;

        public void UpdateLine()
        {
            if (HitObjects.Count == 1)
            {
                Type = LineType.Single;

                var hitObject = HitObjects.First();

                Colour = hitObject.Break ? Color4.OrangeRed : hitObject.DefaultNoteColour;
                Rotation = hitObject.Lane.GetRotationForLane();
            }
            else if (HitObjects.Count > 1)
            {
                int maxDelta = HitObjects.Max(h => getDelta(HitObjects[0], h));
                int minDelta = HitObjects.Min(h => getDelta(HitObjects[0], h));
                var anchor = HitObjects.First(h => getDelta(HitObjects[0], h) == minDelta);
                int delta = maxDelta - minDelta;

                bool allBreaks = HitObjects.All(h => h.Break);

                Type = getLineTypeForDistance(Math.Abs(delta));
                Colour = Color4.Gold;
                Rotation = anchor.Lane.GetRotationForLane() + (delta * 22.5f);
            }

            // Notify the renderer that the line may be updated
            OnLineUpdated?.Invoke(this);
        }

        private void refreshLifetime(ValueChangedEvent<double> valueChangedEvent)
        {
            LifetimeStart = StartTime - AdjustedAnimationDuration;
            LifetimeEnd = StartTime;
        }

        private static LineType getLineTypeForDistance(int distance)
        {
            switch (distance)
            {
                case 0:
                    return LineType.Single;
                case 1:
                    return LineType.OneAway;
                case 2:
                    return LineType.TwoAway;
                case 3:
                    return LineType.ThreeAway;
                default:
                    return LineType.FullCircle;
            }
        }

        private static int getDelta(SentakkiLanedHitObject a, SentakkiLanedHitObject b)
        {
            int delta = b.Lane - a.Lane;
            if (delta > 4) delta -= 8;
            return delta;
        }
    }
}
