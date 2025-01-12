using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Performance;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.UI.Components.HitObjectLine
{
    public partial class LineRenderer : CompositeDrawable
    {
        private readonly Dictionary<double, LineLifetimeEntry> lineEntries = new Dictionary<double, LineLifetimeEntry>();
        private readonly Dictionary<HitObject, IBindable> startTimeMap = new Dictionary<HitObject, IBindable>();

        private readonly Dictionary<LifetimeEntry, DrawableLine> linesInUse = new Dictionary<LifetimeEntry, DrawableLine>();
        private readonly LifetimeEntryManager lifetimeManager = new LifetimeEntryManager();

        private DrawablePool<DrawableLine> linePool = null!;

        public LineRenderer()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Origin = Anchor.Centre;
            lifetimeManager.EntryBecameAlive += onEntryBecameAlive;
            lifetimeManager.EntryBecameDead += onEntryBecameDead;
        }

        [Resolved]
        private DrawableSentakkiRuleset? drawableRuleset { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(linePool = new DrawablePool<DrawableLine>(5));
        }

        protected override bool CheckChildrenLife()
        {
            bool anyAliveChanged = base.CheckChildrenLife();
            anyAliveChanged |= lifetimeManager.Update(Time.Current);
            return anyAliveChanged;
        }

        private void onEntryBecameAlive(LifetimeEntry entry)
        {
            var laneLifetimeEntry = (LineLifetimeEntry)entry;
            var line = linePool.Get();
            line.Entry = laneLifetimeEntry;

            linesInUse[entry] = line;
            AddInternal(line);
        }

        private void onEntryBecameDead(LifetimeEntry entry)
        {
            RemoveInternal(linesInUse[entry], false);
            linesInUse.Remove(entry);
        }

        private void onEntryUpdated(LifetimeEntry entry)
        {
            // We only want to update the drawable when the entry is actually in use
            // This ensures that the drawable gets swapped out with one that uses the correct texture
            // This also resets the colour and rotation if needed
            if (linesInUse.ContainsKey(entry))
            {
                onEntryBecameDead(entry);
                onEntryBecameAlive(entry);
            }
        }

        public void AddHitObject(SentakkiLanedHitObject hitObject)
        {
            var startTimeBindable = hitObject.StartTimeBindable.GetBoundCopy();
            startTimeBindable.ValueChanged += v => onStartTimeChanged(v, hitObject);
            startTimeMap[hitObject] = startTimeBindable;

            addHitObjectToEntry(hitObject.StartTime, hitObject);
        }

        public void RemoveHitObject(SentakkiLanedHitObject hitObject)
        {
            // Ensure that we don't continue to receive time changes
            startTimeMap[hitObject].UnbindAll();

            startTimeMap.Remove(hitObject);

            removeHitObjectFromEntry(hitObject.StartTime, hitObject);
        }

        private void onStartTimeChanged(ValueChangedEvent<double> valueChangedEvent, SentakkiLanedHitObject hitObject)
        {
            removeHitObjectFromEntry(valueChangedEvent.OldValue, hitObject);
            addHitObjectToEntry(valueChangedEvent.NewValue, hitObject);
        }

        private void removeHitObjectFromEntry(double entryTime, SentakkiLanedHitObject hitObject)
        {
            // Safety check to ensure the a line entry actually exists
            if (lineEntries.TryGetValue(entryTime, out var line))
            {
                line.Remove(hitObject);

                // Remove this entry completely if there aren't any hitObjects using it
                if (!line.HitObjects.Any())
                {
                    lifetimeManager.RemoveEntry(lineEntries[entryTime]);
                    lineEntries.Remove(entryTime);
                }
            }
        }

        private void addHitObjectToEntry(double entryTime, SentakkiLanedHitObject hitObject)
        {
            // Create new line entry for this entryTime if none exists
            if (!lineEntries.ContainsKey(entryTime))
            {
                var newEntry = new LineLifetimeEntry(drawableRuleset, entryTime);
                lineEntries[entryTime] = newEntry;
                lifetimeManager.AddEntry(newEntry);

                // We want to listen in on line changes in case we need to swap out colours/drawables
                newEntry.OnLineUpdated += onEntryUpdated;
            }

            lineEntries[entryTime].Add(hitObject);
        }
    }
}
