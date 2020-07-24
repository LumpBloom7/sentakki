using osu.Game.Rulesets.Scoring;
using osuTK;
using osu.Framework.Graphics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Bindables;
using osu.Game.Skinning;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Screens.Play;
using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideNode : DrawableSentakkiHitObject
    {

        [Resolved(canBeNull: true)]
        private GameplayClock gameplayClock { get; set; }

        private readonly Bindable<bool> userPositionalHitSounds = new Bindable<bool>(false);
        private SkinnableSound slideSound;

        public override bool DisplayResult => (HitObject as Slide.SlideNode).Progress == 1;
        public override bool HandlePositionalInput => true;
        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;
        protected DrawableSlide Slide;

        protected int ThisIndex;
        public DrawableSlideNode(Slide.SlideNode node, DrawableSlide slideNote)
            : base(node)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Slide = slideNote;
            RelativeSizeAxes = Axes.None;
            Position = slideNote.Slidepath.Path.PositionAt((HitObject as Slide.SlideNode).Progress);
            Size = new Vector2(160);
            CornerExponent = 2f;
            CornerRadius = 80;
            Masking = true;
        }
        protected override void LoadComplete()
        {
            base.LoadComplete();
            ThisIndex = Slide.SlideNodes.IndexOf(this);
            if (ThisIndex == 0) AddInternal(slideSound = new SkinnableSound(new SampleInfo("slide")));

            OnNewResult += (DrawableHitObject hitObject, JudgementResult result) =>
            {
                Slide.Slidepath.Progress = (HitObject as Slide.SlideNode).Progress;
            };
            OnRevertResult += (DrawableHitObject hitObject, JudgementResult result) =>
            {
                Slide.Slidepath.Progress = ThisIndex > 0 ? (Slide.SlideNodes[ThisIndex - 1].HitObject as Slide.SlideNode).Progress : 0;
            };
        }

        protected bool IsHittable => ThisIndex < 2 || Slide.SlideNodes[ThisIndex - 2].IsHit;
        private bool isTailNode => (HitObject as Slide.SlideNode).IsTailNote;

        protected void HitPreviousNodes(bool successful = false)
        {
            foreach (var node in Slide.SlideNodes)
            {
                if (node == this) return;
                if (!node.Result.HasResult)
                    node.forceJudgement(successful);
            }
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager osuConfig, SentakkiRulesetConfigManager sentakkiConfig)
        {
            osuConfig.BindWith(OsuSetting.PositionalHitSounds, userPositionalHitSounds);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (!userTriggered)
            {
                if (timeOffset > 0 && Auto)
                {
                    ApplyResult(r => r.Type = HitResult.Perfect);
                    HitPreviousNodes(true);
                }
                if (isTailNode && !HitObject.HitWindows.CanBeHit(timeOffset))
                {
                    ApplyResult(r => r.Type = IsHittable ? HitResult.Good : HitResult.Miss);
                    HitPreviousNodes();
                }
                return;
            }
            if (!IsHittable)
                return;


            HitResult result;

            if (isTailNode)
            {
                result = HitObject.HitWindows.ResultFor(timeOffset);
                if (result == HitResult.None)
                    result = HitResult.Meh;
            }
            else
                result = HitResult.Perfect;

            ApplyResult(r => r.Type = result);

            HitPreviousNodes(result > HitResult.Miss);
        }
        public void UpdateResult() => base.UpdateResult(true);

        // Forces this object to have a result.
        private void forceJudgement(bool successful = false) => ApplyResult(r => r.Type = successful ? HitResult.Perfect : HitResult.Miss);

        protected override void Update()
        {
            base.Update();
            if (Time.Current >= Slide.HitObject.StartTime)
                if (IsHovered)
                    if (SentakkiActionInputManager.PressedActions.Any())
                        UpdateResult(true);
        }

        public override void PlaySamples()
        {
            base.PlaySamples();
            if (slideSound != null && Result.Type != HitResult.Miss && (!gameplayClock?.IsSeeking ?? false))
            {
                const float balance_adjust_amount = 0.4f;
                slideSound.Balance.Value = balance_adjust_amount * (userPositionalHitSounds.Value ? SamplePlaybackPosition - 0.5f : 0);
                slideSound.Play();
            }
        }
        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);

            switch (state)
            {
                case ArmedState.Hit:
                    Slide.Slidepath.Progress = (HitObject as Slide.SlideNode).Progress;
                    break;
            }
        }
    }
}
