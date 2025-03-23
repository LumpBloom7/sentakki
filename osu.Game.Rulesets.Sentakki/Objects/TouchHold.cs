using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public partial class TouchHold : SentakkiHitObject, IHasDuration
    {
        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        private HitObjectProperty<Vector2> position;

        public Bindable<Vector2> PositionBindable => position.Bindable;

        public Vector2 Position
        {
            get => position.Value;
            set => position.Value = value;
        }

        public float X
        {
            get => Position.X;
            set => Position = new Vector2(value, Position.Y);
        }

        public float Y
        {
            get => Position.Y;
            set => Position = new Vector2(Position.X, value);
        }

        public override bool Ex
        {
            get => base.Ex;
            set { } // TouchHold doesn't support EX note (how would that even work?!)
        }

        private HitObjectProperty<IReadOnlyList<Color4>> colourPalette = new(DefaultPalette);
        public Bindable<IReadOnlyList<Color4>> ColourPaletteBindable => colourPalette.Bindable;

        public IReadOnlyList<Color4> ColourPalette
        {
            get => colourPalette.Value;
            set => colourPalette.Value = value;
        }

        public double Duration { get; set; }

        protected override HitWindows CreateHitWindows() => new SentakkiEmptyHitWindows();

        public override IList<HitSampleInfo> AuxiliarySamples => CreateHoldSample();

        public HitSampleInfo[] CreateHoldSample()
        {
            var referenceSample = Samples.FirstOrDefault();

            if (referenceSample == null)
                return [];

            return [referenceSample.With("spinnerspin")];
        }
    }

    // Static stuff
    public partial class TouchHold : SentakkiHitObject, IHasDuration
    {
        public static readonly IReadOnlyList<Color4> DefaultPalette;
        public static readonly IReadOnlyList<Color4> BreakPalette;

        static TouchHold()
        {
            OsuColour colours = new OsuColour();
            DefaultPalette = [
                colours.Red,
                colours.Yellow,
                colours.Green,
                colours.Blue,
            ];

            BreakPalette = [
                Color4.OrangeRed,
                Colour4.FromHex("#802200"),
                Color4.OrangeRed,
                Colour4.FromHex("#802200"),
            ];
        }
    }
}
