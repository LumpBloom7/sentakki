using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Performance;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI.Components.HitObjectLine
{
    public class LineLifetimeEntry : LifetimeEntry
    {
        public enum LineType
        {
            Single,
            OneAway,
            TwoAway,
            FullCircle,
        }

        public string GetLineTexturePath()
        {
            switch (Type)
            {
                case LineType.Single:
                    return "Lines/90";
                case LineType.OneAway:
                    return "Lines/135";
                case LineType.TwoAway:
                    return "Lines/225";
                case LineType.FullCircle:
                    return "Lines/360";
                default:
                    return "";
            }
        }

        public BindableDouble AnimationDuration = new BindableDouble(1000);
        public double AdjustedAnimationDuration => AnimationDuration.Value * GameplaySpeed;

        public double GameplaySpeed => drawableRuleset?.GameplaySpeed ?? 1;

        private readonly DrawableSentakkiRuleset drawableRuleset;

        public double StartTime { get; private set; }

        public LineLifetimeEntry(BindableDouble AnimationDuration, DrawableSentakkiRuleset drawableSentakkiRuleset, double startTime)
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

        private void onLaneChanged(ValueChangedEvent<int> obj) => UpdateLine();

        private void onBreakChanged(ValueChangedEvent<bool> obj) => UpdateLine();

        public void UpdateLine()
        {
            if (HitObjects.Count == 1)
            {
                Type = LineType.Single;

                var hitObject = HitObjects.First();

                Colour = hitObject.Break ? Color4.OrangeRed : hitObject.DefaultNoteColour;
                Rotation = hitObject.Lane.GetRotationForLane();
            }
        }

        private void refreshLifetime(ValueChangedEvent<double> valueChangedEvent)
        {
            LifetimeStart = StartTime - AdjustedAnimationDuration;
            LifetimeEnd = StartTime;
        }
    }
}
