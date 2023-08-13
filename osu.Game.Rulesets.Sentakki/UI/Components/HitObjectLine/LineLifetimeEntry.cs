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
        private readonly BindableDouble animationDuration = new BindableDouble(1000);
        public double AdjustedAnimationDuration => animationDuration.Value * GameplaySpeed;

        public double GameplaySpeed => drawableRuleset?.GameplaySpeed ?? 1;

        private readonly DrawableSentakkiRuleset? drawableRuleset;

        public double StartTime { get; private set; }

        public LineLifetimeEntry(BindableDouble animationDuration, DrawableSentakkiRuleset? drawableSentakkiRuleset, double startTime)
        {
            StartTime = startTime;
            drawableRuleset = drawableSentakkiRuleset;
            this.animationDuration.BindTo(animationDuration);
            this.animationDuration.BindValueChanged(refreshLifetime, true);
        }

        public List<SentakkiLanedHitObject> HitObjects = new List<SentakkiLanedHitObject>();

        public float AngleRange { get; private set; }
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
                AngleRange = 0.25f;

                var hitObject = HitObjects.First();

                Colour = hitObject.Break ? Color4.OrangeRed : hitObject.DefaultNoteColour;
                Rotation = hitObject.Lane.GetRotationForLane() - 45;
            }
            else if (HitObjects.Count > 1)
            {
                int maxDelta = HitObjects.Max(h => getDelta(HitObjects[0], h));
                int minDelta = HitObjects.Min(h => getDelta(HitObjects[0], h));
                var anchor = HitObjects.First(h => getDelta(HitObjects[0], h) == minDelta);
                int delta = maxDelta - minDelta;

                Colour = Color4.Gold;

                int angleRange = delta == 4 ? 360 : (90 + (45 * delta));

                AngleRange = angleRange / 360f;

                Rotation = anchor.Lane.GetRotationForLane() + (delta * 22.5f) - (angleRange / 2f);
            }

            // Notify the renderer that the line may be updated
            OnLineUpdated?.Invoke(this);
        }

        private void refreshLifetime(ValueChangedEvent<double> valueChangedEvent)
        {
            LifetimeStart = StartTime - AdjustedAnimationDuration;
            LifetimeEnd = StartTime;
        }

        private static int getDelta(SentakkiLanedHitObject a, SentakkiLanedHitObject b)
        {
            int delta = b.Lane - a.Lane;
            if (delta > 4) delta -= 8;
            return delta;
        }
    }
}
