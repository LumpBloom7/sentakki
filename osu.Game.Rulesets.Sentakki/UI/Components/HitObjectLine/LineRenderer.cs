using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Performance;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.UI.Components.HitObjectLine
{
    public partial class LineRenderer : CompositeDrawable
    {
        private readonly List<LineLifetimeEntry> lineLifetimeEntries = [];
        private readonly Dictionary<SentakkiLanedHitObject, IBindable> startTimeMap = new Dictionary<SentakkiLanedHitObject, IBindable>();
        private readonly Dictionary<LifetimeEntry, DrawableLine> linesInUse = new Dictionary<LifetimeEntry, DrawableLine>();
        private readonly LifetimeEntryManager lifetimeManager = new LifetimeEntryManager();

        private readonly Stack<LineLifetimeEntry> lifetimeEntryPool = [];

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

        public static IEnumerable<List<SentakkiLanedHitObject>> createTimingGroup(IEnumerable<SentakkiLanedHitObject> hitObjects)
        {
            var hitObjectsCategory = hitObjects.OrderBy(ho => ho.StartTime).GroupBy(h => new { isSlide = h is SlideBody });

            foreach (var hitobjectCategory in hitObjectsCategory)
            {
                List<SentakkiLanedHitObject> timedGroup = [];
                double lastTime = double.MinValue;

                foreach (var ho in hitobjectCategory)
                {
                    double time = ho.StartTime;

                    if ((time - lastTime) >= 1 && timedGroup.Count > 0)
                    {
                        yield return timedGroup;
                        timedGroup = [];
                    }

                    timedGroup.Add(ho);
                    lastTime = time;
                }

                if (timedGroup.Count > 0)
                    yield return timedGroup;
            }
        }

        private double medianTime(List<SentakkiLanedHitObject> lanedHitObjects)
        {
            int midpoint = lanedHitObjects.Count / 2;
            if (lanedHitObjects.Count > 2 && lanedHitObjects.Count % 2 == 0)
                return (lanedHitObjects[midpoint].StartTime + lanedHitObjects[midpoint - 1].StartTime) / 2;

            return lanedHitObjects[midpoint].StartTime;
        }

        private void refreshLifetimeEntries()
        {
            lifetimeManager.ClearEntries();

            foreach (var existingLifetimeEntry in lineLifetimeEntries)
            {
                existingLifetimeEntry.Clear();
                lifetimeEntryPool.Push(existingLifetimeEntry);
            }

            lineLifetimeEntries.Clear();

            foreach (var group in createTimingGroup(startTimeMap.Keys))
            {
                if (!lifetimeEntryPool.TryPop(out var newEntry))
                    newEntry = new LineLifetimeEntry(drawableRuleset);

                lineLifetimeEntries.Add(newEntry);
                newEntry.StartTime = medianTime(group);

                foreach (var ho in group)
                    newEntry.Add(ho);

                lifetimeManager.AddEntry(newEntry);
            }
        }

        public void AddHitObject(SentakkiLanedHitObject hitObject)
        {
            var startTimeBindable = hitObject.StartTimeBindable.GetBoundCopy();
            startTimeBindable.ValueChanged += v => onStartTimeChanged(v, hitObject);
            startTimeMap[hitObject] = startTimeBindable;

            refreshLifetimeEntries();
        }

        public void RemoveHitObject(SentakkiLanedHitObject hitObject)
        {
            if (!startTimeMap.TryGetValue(hitObject, out var bindable))
                return;

            // Ensure that we don't continue to receive time changes
            bindable.UnbindAll();
            startTimeMap.Remove(hitObject);

            refreshLifetimeEntries();
        }

        private void onStartTimeChanged(ValueChangedEvent<double> valueChangedEvent, SentakkiLanedHitObject hitObject)
            => refreshLifetimeEntries();
    }
}
