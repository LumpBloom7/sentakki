using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Performance;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI.Components.HitObjectLine;

public class LineLifetimeEntry : LifetimeEntry
{
    private readonly BindableDouble animationDuration = new BindableDouble(1000);

    public double StartTime { get; }

    public LineLifetimeEntry(DrawableSentakkiRuleset? drawableSentakkiRuleset, double startTime)
    {
        StartTime = startTime;

        animationDuration.TryBindTo(drawableSentakkiRuleset?.AdjustedAnimDuration);
        animationDuration.BindValueChanged(refreshLifetime, true);
    }

    public List<SentakkiLanedHitObject> HitObjects = [];

    public float AngleRange { get; private set; }
    public ColourInfo Colour { get; private set; }
    public float Rotation { get; private set; }

    public void Add(SentakkiLanedHitObject hitObject)
    {
        hitObject.BreakBindable.ValueChanged += onBreakChanged;
        hitObject.ColourBindable.ValueChanged += onColorChanged;
        HitObjects.AddInPlace(hitObject, Comparer<SentakkiLanedHitObject>.Create((lhs, rhs) => lhs.Lane.CompareTo(rhs.Lane)));
        updateLine();
    }

    public void Remove(SentakkiLanedHitObject hitObject)
    {
        hitObject.BreakBindable.ValueChanged -= onBreakChanged;
        HitObjects.Remove(hitObject);
        updateLine();
    }

    private void onBreakChanged(ValueChangedEvent<bool> obj) => updateLine();
    private void onColorChanged(ValueChangedEvent<Color4> obj) => updateLine();

    public Action<LineLifetimeEntry>? OnLineUpdated = null!;

    private void updateLine()
    {
        switch (HitObjects.Count)
        {
            case 1:
            {
                AngleRange = 0.25f;

                var hitObject = HitObjects.First();

                Colour = hitObject.NoteColour;
                Rotation = hitObject.Lane.GetRotationForLane() - 45;
                break;
            }

            case > 1:
            {
                int maxDelta = HitObjects.Max(h => getDelta(HitObjects[0], h));
                int minDelta = HitObjects.Min(h => getDelta(HitObjects[0], h));
                var anchor = HitObjects.First(h => getDelta(HitObjects[0], h) == minDelta);
                int delta = maxDelta - minDelta;

                Colour = Color4.Gold;

                int angleRange = delta == 4 ? 360 : 90 + 45 * delta;

                AngleRange = angleRange / 360f;

                if (delta == 4)
                {
                    Rotation = HitObjects.First().Lane.GetRotationForLane() - 180;
                }
                else
                {
                    Rotation = anchor.Lane.GetRotationForLane() + delta * 22.5f - angleRange / 2f;
                }

                break;
            }
        }

        // Notify the renderer that the line may be updated
        OnLineUpdated?.Invoke(this);
    }

    private void refreshLifetime(ValueChangedEvent<double> valueChangedEvent)
    {
        LifetimeStart = StartTime - valueChangedEvent.NewValue;
        LifetimeEnd = StartTime;
    }

    private static int getDelta(SentakkiLanedHitObject a, SentakkiLanedHitObject b)
    {
        int delta = b.Lane - a.Lane;
        if (delta > 4) delta -= 8;
        return delta;
    }
}
