using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Statistics;

public partial class JudgementChart : TableContainer
{
    private static readonly Color4 accent_color = Color4Extensions.FromHex("#66FFCC");

    private static readonly (string, Func<HitEvent, bool>)[] hitObjectTypes =
    {
            ("Tap", e => e.HitObject is Tap x && !x.Break),
            ("Hold", e => ((e.HitObject is Hold.HoldHead) && !((SentakkiLanedHitObject)e.HitObject).Break) || e.HitObject is Hold),
            ("Slide", e => e.HitObject is SlideBody x),
            ("Touch", e => e.HitObject is Touch),
            ("Touch Hold", e => e.HitObject is TouchHold),
            // Note Hold and Slide breaks are applied to child objects, not itself.
            ("Break", e => e.HitObject is SentakkiLanedHitObject x && (x is not Hold) && (x is not Slide) && x.Break),
    };

    private static readonly HitResult[] valid_results = new HitResult[]{
        HitResult.Perfect,
        HitResult.Great,
        HitResult.Good,
        HitResult.Ok,
        HitResult.Miss
    };

    public JudgementChart(IReadOnlyList<HitEvent> hitEvents)
    {
        var columns = new TableColumn[7];
        Array.Fill(columns, new TableColumn(null, Anchor.Centre, new Dimension(GridSizeMode.Distributed)));
        Columns = columns;

        RowSize = new Dimension(GridSizeMode.Distributed);

        var content = new Drawable[6, 7];

        for (int i = 0; i < hitObjectTypes.Length; ++i)
        {
            var entry = hitObjectTypes[i];

            Dictionary<HitResult, int> results = collectHitResultsFor(hitEvents.Where(entry.Item2));

            int sum = results.Sum(kvp => kvp.Value);

            bool perfected = sum == results.GetValueOrDefault(HitResult.Perfect) + results.GetValueOrDefault(HitResult.Great);
            bool critPerfect = sum == results.GetValueOrDefault(HitResult.Perfect);

            Color4 specialColor = Color4.White;

            if (critPerfect)
                specialColor = HitResult.Perfect.GetColorForSentakkiResult();
            else if (perfected)
                specialColor = HitResult.Great.GetColorForSentakkiResult();


            // The alpha will be used to "disable" an hitobject entry if they don't exist
            float commonAlpha = sum == 0 ? 0.1f : 1;

            content[i, 0] = new OsuSpriteText()
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Font = OsuFont.Torus.With(size: 20, weight: FontWeight.Bold),
                Text = entry.Item1,
                Colour = specialColor,
                Alpha = commonAlpha
            };

            // Total notes
            content[i, 1] = new TotalNoteCounter(sum, true)
            {
                Colour = accent_color,
                Alpha = commonAlpha
            };

            for (int j = 0; j < valid_results.Length; ++j)
            {
                content[i, 2 + j] = new TotalNoteCounter(results.GetValueOrDefault(valid_results[j]))
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Colour = valid_results[j].GetColorForSentakkiResult(),
                    Alpha = commonAlpha
                };
            }
        }

        Content = content;
    }

    protected override Drawable CreateHeader(int index, TableColumn? column)
    {
        if (index == 0)
            return null!;

        string text = index == 1 ? "TOTAL" : valid_results[index - 2].GetDisplayNameForSentakkiResult();
        var Colour = index == 1 ? accent_color : valid_results[index - 2].GetColorForSentakkiResult();

        return new OsuSpriteText()
        {
            Origin = Anchor.Centre,
            Anchor = Anchor.Centre,
            Font = OsuFont.Torus.With(size: 20, weight: FontWeight.Bold),
            Text = text,
            Colour = Colour
        };
    }

    private static Dictionary<HitResult, int> collectHitResultsFor(IEnumerable<HitEvent> hitEvents)
    {
        var resultGroups = hitEvents.GroupBy(h => h.Result);

        Dictionary<HitResult, int> counts = new();

        foreach (var he in resultGroups)
            counts[he.Key] = he.Count();

        return counts;
    }

    private partial class TotalNoteCounter : RollingCounter<int>
    {
        protected override double RollingDuration => 3000;

        protected override Easing RollingEasing => Easing.OutPow10;

        protected override LocalisableString FormatCount(int count) => count.ToString("N0");

        private readonly int totalValue;
        private readonly bool bold;

        public TotalNoteCounter(int value, bool bold = false)
        {
            Current = new Bindable<int> { Value = 0 };
            totalValue = value;
            this.bold = bold;
        }

        protected override OsuSpriteText CreateSpriteText() => new OsuSpriteText
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Font = OsuFont.Torus.With(size: 20, weight: bold ? FontWeight.Bold : FontWeight.Regular),
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.TransformBindableTo(Current, totalValue);
        }
    }
}
