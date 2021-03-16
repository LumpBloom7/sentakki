using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Performance;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Skinning;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI.Components.HitObjectLine
{
    public class HitObjectLineRenderer : CompositeDrawable
    {
        private readonly Dictionary<double, LineLifetimeEntry> lineEntries = new Dictionary<double, LineLifetimeEntry>();
        private readonly Dictionary<HitObject, IBindable> startTimeMap = new Dictionary<HitObject, IBindable>();

        private readonly Dictionary<LifetimeEntry, HitObjectLine> linesInUse = new Dictionary<LifetimeEntry, HitObjectLine>();
        private readonly LifetimeEntryManager lifetimeManager = new LifetimeEntryManager();

        private readonly Dictionary<LineType, DrawablePool<HitObjectLine>> linePools = new Dictionary<LineType, DrawablePool<HitObjectLine>>();

        public HitObjectLineRenderer()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            lifetimeManager.EntryBecameAlive += onEntryBecameAlive;
            lifetimeManager.EntryBecameDead += onEntryBecameDead;
        }

        private readonly BindableDouble animationDuration = new BindableDouble(1000);

        [Resolved(canBeNull: true)]
        private DrawableSentakkiRuleset drawableRuleset { get; set; }


        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfigs)
        {
            sentakkiConfigs?.BindWith(SentakkiRulesetSettings.AnimationDuration, animationDuration);

            foreach (var type in Enum.GetValues(typeof(LineType)).OfType<LineType>())
                linePools.Add(type, new DrawableLinePool(type));

            AddRangeInternal(linePools.Values);
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
            var line = linePools[laneLifetimeEntry.Type].Get();
            line.Apply(laneLifetimeEntry);

            linesInUse[entry] = line;
            AddInternal(line);
        }

        private void onEntryBecameDead(LifetimeEntry entry)
        {
            RemoveInternal(linesInUse[entry]);
            linesInUse.Remove(entry);
        }

        public void AddHitObject(SentakkiLanedHitObject hitObject)
        {
            var startTimeBindable = hitObject.StartTimeBindable.GetBoundCopy();
            startTimeBindable.ValueChanged += _ => onStartTimeChanged(hitObject);
            startTimeMap[hitObject] = startTimeBindable;

            addEntry(hitObject);
        }

        public void RemoveHitObject(SentakkiLanedHitObject hitObject)
        {
            startTimeMap.Remove(hitObject);
            removeEntry(hitObject);
        }

        private void onStartTimeChanged(SentakkiLanedHitObject hitObject)
        {
            removeEntry(hitObject);
            addEntry(hitObject);
        }

        private void removeEntry(SentakkiLanedHitObject hitObject)
        {
            if (lineEntries.TryGetValue(hitObject.StartTime, out var line))
            {
                line.HitObjects.Remove(hitObject);

                if (!line.HitObjects.Any())
                {
                    lifetimeManager.RemoveEntry(lineEntries[hitObject.StartTime]);
                    lineEntries.Remove(hitObject.StartTime);
                }
            }
        }

        private void addEntry(SentakkiLanedHitObject hitObject)
        {
            if (!lineEntries.ContainsKey(hitObject.StartTime))
            {
                var newEntry = new LineLifetimeEntry(animationDuration, drawableRuleset, hitObject.StartTime);
                lineEntries[hitObject.StartTime] = newEntry;
                lifetimeManager.AddEntry(newEntry);
            }
            lineEntries[hitObject.StartTime].Add(hitObject);
        }

        public class DrawableLinePool : DrawablePool<HitObjectLine>
        {
            private readonly LineType type;

            public DrawableLinePool(LineType type)
                : base(5)
            {
                this.type = type;
            }

            protected override HitObjectLine CreateNewDrawable()
            {
                return new HitObjectLine { Type = type };
            }
        }
    }
}
