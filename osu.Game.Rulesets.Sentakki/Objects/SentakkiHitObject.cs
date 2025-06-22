using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Utils;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public abstract class SentakkiHitObject : HitObject
    {
        public int ScoreWeighting => Break ? 5 : BaseScoreWeighting;

        protected virtual int BaseScoreWeighting => 1;

        protected virtual bool PlaysBreakSample => true;

        private HitObjectProperty<bool> breakState;

        public Bindable<bool> BreakBindable => breakState.Bindable;

        public bool Break
        {
            get => breakState.Value;
            set => breakState.Value = value;
        }

        protected SentakkiHitObject()
        {
            // We initialize the note colour to the default value first for test scenes
            // The colours during gameplay will be set during beatmap post-process
            colour.Value = DefaultNoteColour;
        }

        public override Judgement CreateJudgement() => new SentakkiJudgement();

        private HitObjectProperty<Color4> colour;

        [JsonIgnore]
        public Bindable<Color4> ColourBindable => colour.Bindable;

        [JsonIgnore]
        public Color4 NoteColour
        {
            get => colour.Value;
            set => colour.Value = value;
        }

        private HitObjectProperty<bool> exState;

        public Bindable<bool> ExBindable => exState.Bindable;

        public virtual bool Ex
        {
            get => exState.Value;
            set => exState.Value = value;
        }

        [JsonIgnore]
        public virtual Color4 DefaultNoteColour => Color4Extensions.FromHex("FF0064");

        protected override HitWindows CreateHitWindows() => new SentakkiTapHitWindows();

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            for (int i = 1; i < ScoreWeighting; ++i)
                AddNested(new ScorePaddingObject { StartTime = this.GetEndTime(), ParentMaximumJudgementOffset = MaximumJudgementOffset });
        }

        public HitSampleInfo[] CreateBreakSample()
        {
            if (!PlaysBreakSample || !Break)
                return Array.Empty<HitSampleInfo>();

            return new[]
            {
                new BreakSample( CreateHitSampleInfo())
            };
        }

        public override IList<HitSampleInfo> AuxiliarySamples =>
            (Break && PlaysBreakSample) ? [new BreakSample(CreateHitSampleInfo())] : [];

        public class BreakSample : HitSampleInfo
        {
            public override IEnumerable<string> LookupNames
            {
                get
                {
                    foreach (string name in base.LookupNames)
                        yield return name;

                    foreach (string name in base.LookupNames)
                        yield return name.Replace("-max", string.Empty);
                }
            }

            public BreakSample(HitSampleInfo sampleInfo)
                : base("spinnerbonus-max", sampleInfo.Bank, sampleInfo.Suffix, sampleInfo.Volume)
            {
            }
        }

        // This special hitsample is used for Sentakki specific samples, with doesn't have bank specific variants
        public class SentakkiHitSampleInfo : HitSampleInfo, IEquatable<SentakkiHitSampleInfo>
        {
            public SentakkiHitSampleInfo(string name, int volume = 0)
                : base(name, volume: volume)
            {
            }

            public override IEnumerable<string> LookupNames
            {
                get
                {
                    yield return "Gameplay/" + Name;
                }
            }

#nullable enable
            public override HitSampleInfo With(Optional<string> newName = default, Optional<string> newBank = default, Optional<string?> newSuffix = default, Optional<int> newVolume = default, Optional<bool> newEditorAutoBank = default)
            {
                return new SentakkiHitSampleInfo(newName.GetOr(Name), newVolume.GetOr(Volume));
            }
#nullable disable

            public bool Equals(SentakkiHitSampleInfo other)
            {
                return other != null && Name == other.Name;
            }

            public override bool Equals(object obj)
            {
                return obj is SentakkiHitSampleInfo s && Equals(s);
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }
        }
    }
}
