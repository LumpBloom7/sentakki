using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Extensions;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Argon;

public partial class ArgonSentakkiJudgementPiece : TextJudgementPiece, IAnimatableJudgement
{
    [Resolved]
    private OsuColour colours { get; set; } = null!;

    private RingExplosion? ringExplosion;

    public ArgonSentakkiJudgementPiece(HitResult result) : base(result)
    {
        AutoSizeAxes = Axes.Both;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        JudgementText.Text = SentakkiExtensions.GetDisplayNameForSentakkiResult(Result).ToUpperInvariant();
        JudgementText.Colour = colours.ForSentakkiResult(Result);

        if (!Result.IsHit())
            return;

        AddInternal(ringExplosion = new RingExplosion(Result)
        {
            Colour = JudgementText.Colour.AverageColour
        });
    }

    protected override SpriteText CreateJudgementText() =>
        new OsuSpriteText
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Blending = BlendingParameters.Additive,
            Font = OsuFont.Default.With(size: 28, weight: FontWeight.Bold),
        };

    public void PlayAnimation()
    {
        switch (Result)
        {
            case HitResult.None:
                this.FadeOutFromOne(800);
                break;

            case HitResult.Miss:
                this.ScaleTo(1.6f);
                this.ScaleTo(1, 100, Easing.In);

                this.MoveTo(Vector2.Zero);
                this.MoveToOffset(new Vector2(0, 50), 800, Easing.InQuint);

                this.RotateTo(0);
                this.RotateTo(40, 800, Easing.InQuint);

                this.FadeOutFromOne(800);
                break;

            default:
                ringExplosion?.PlayAnimation();
                this.ScaleTo(0.8f);
                this.ScaleTo(1, 50, Easing.OutElastic);

                this.Delay(50)
                    .ScaleTo(0.75f, 300)
                    .FadeOut(300);

                break;
        }
    }

    public Drawable? GetAboveHitObjectsProxiedContent() => JudgementText.CreateProxy();

    private partial class RingExplosion : CompositeDrawable
    {
        private readonly float travel = 52;

        public RingExplosion(HitResult result)
        {
            const float thickness = 4;

            const float small_size = 9;
            const float large_size = 14;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Blending = BlendingParameters.Additive;

            int countSmall = 0;
            int countLarge = 0;

            switch (result)
            {
                case HitResult.Meh:
                    countSmall = 3;
                    travel *= 0.3f;
                    break;

                case HitResult.Ok:
                case HitResult.Good:
                    countSmall = 4;
                    travel *= 0.6f;
                    break;

                case HitResult.Great:
                case HitResult.Perfect:
                    countSmall = 4;
                    countLarge = 4;
                    break;
            }

            for (int i = 0; i < countSmall; i++)
                AddInternal(new RingPiece(thickness) { Size = new Vector2(small_size) });

            for (int i = 0; i < countLarge; i++)
                AddInternal(new RingPiece(thickness) { Size = new Vector2(large_size) });
        }

        public void PlayAnimation()
        {
            foreach (var c in InternalChildren)
            {
                const float start_position_ratio = 0.3f;

                float direction = RNG.NextSingle(0, 360);
                float distance = RNG.NextSingle(travel / 2, travel);

                c.MoveTo(new Vector2(
                    MathF.Cos(direction) * distance * start_position_ratio,
                    MathF.Sin(direction) * distance * start_position_ratio
                ));

                c.MoveTo(new Vector2(
                    MathF.Cos(direction) * distance,
                    MathF.Sin(direction) * distance
                ), 600, Easing.OutQuint);
            }

            this.FadeOutFromOne(1000, Easing.OutQuint);
        }

        public partial class RingPiece : CircularContainer
        {
            public RingPiece(float thickness = 9)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Masking = true;
                BorderThickness = thickness;
                BorderColour = Color4.White;

                Child = new Box
                {
                    AlwaysPresent = true,
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both
                };
            }
        }
    }
}
