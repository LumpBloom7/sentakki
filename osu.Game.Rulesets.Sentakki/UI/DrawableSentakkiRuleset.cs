using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Input.Bindings;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Sentakki.UI
{
    [Cached]
    public partial class DrawableSentakkiRuleset : DrawableRuleset<SentakkiHitObject>, IKeyBindingHandler<GlobalAction>
    {
        private SlideFanChevrons slideFanChevronsTextures = null!;

        public DrawableSentakkiRuleset(SentakkiRuleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod>? mods)
            : base(ruleset, beatmap, mods)
        {
            foreach (var mod in Mods.OfType<IApplicableToTrack>())
                mod.ApplyToTrack(speedAdjustmentTrack);
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            // We create and render the FanChevron outside of the playfield
            // This is to ensure that the fan chevrons doesn't get affected by Playfield transforms (avoiding excessive buffer allocs/deallocs)
            // FanSlides will use BufferedContainerView to show the chevrons
            dependencies.CacheAs(slideFanChevronsTextures = new SlideFanChevrons());

            return dependencies;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Config.BindWith(SentakkiRulesetSettings.LaneInputMode, laneInputMode);

            FrameStableComponents.Add(slideFanChevronsTextures);

            Config.BindWith(SentakkiRulesetSettings.AnimationDuration, configEntrySpeed);
            Config.BindWith(SentakkiRulesetSettings.TouchAnimationDuration, configTouchEntrySpeed);

            configEntrySpeed.BindValueChanged(v => this.TransformTo(nameof(smoothAnimDuration), ComputeLaneNoteEntryTime(v.NewValue), 200, Easing.OutQuint));
            configTouchEntrySpeed.BindValueChanged(v => this.TransformTo(nameof(smoothTouchAnimDuration), ComputeTouchNoteEntryTime(v.NewValue), 200, Easing.OutQuint));
            smoothAnimDuration = ComputeLaneNoteEntryTime(configEntrySpeed.Value);
            smoothTouchAnimDuration = ComputeTouchNoteEntryTime(configTouchEntrySpeed.Value);
        }

        private readonly Bindable<float> configEntrySpeed = new BindableFloat(2.0f)
        {
            MinValue = 1.0f,
            MaxValue = 10.5f,
        };

        private double smoothAnimDuration = 1000;
        public readonly Bindable<double> AdjustedAnimDuration = new Bindable<double>(1000);

        private readonly Bindable<float> configTouchEntrySpeed = new Bindable<float>(2.0f);
        private double smoothTouchAnimDuration = 1000;
        public readonly Bindable<double> AdjustedTouchAnimDuration = new Bindable<double>(1000);

        // Computes the total animation time (in ms) for lane notes from a speedValue
        public static double ComputeLaneNoteEntryTime(float speedValue)
        {
            // The formula is (2400 / (x + 1)) * 2 (We multiply by two to include fade in, which would be the same time)

            if (speedValue > 10) // Sonic speed is equivalent to note speed 49
                return 96f;

            if (speedValue < 1)
                return 2400f;

            return 2 * (2400 / (speedValue + 1));
        }

        // Computes the total animation time (in ms) for touch notes from a speedValue
        public static double ComputeTouchNoteEntryTime(float speedValue)
        {
            // The formula is (9600 / (2x + 5)) * 1.25 (We multiply by 1.25 to include fade in, which would a quarter of the time)

            if (speedValue > 10) // Sonic speed is equivalent to touch note speed 97.5
                return 60;

            if (speedValue < 1)
                return 9600 / 7 * 1.25f;

            return 9600 / (2 * speedValue + 5) * 1.25f;
        }

        protected override void Update()
        {
            base.Update();
            updateAnimationDurations();
        }

        private void updateAnimationDurations()
        {
            AdjustedAnimDuration.Value = smoothAnimDuration * GameplaySpeed;
            AdjustedTouchAnimDuration.Value = smoothTouchAnimDuration * GameplaySpeed;
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.DecreaseScrollSpeed:
                    configEntrySpeed.Value -= 0.5f;
                    return true;
                case GlobalAction.IncreaseScrollSpeed:
                    configEntrySpeed.Value += 0.5f;
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e) { }

        // Gameplay speed specifics
        private readonly Track speedAdjustmentTrack = new TrackVirtual(0);

        public double GameplaySpeed => speedAdjustmentTrack.Rate;

        protected new SentakkiRulesetConfigManager Config => (SentakkiRulesetConfigManager)base.Config;

        // Input specifics (sensor/button) for replay and gameplay
        private readonly Bindable<LaneInputMode> laneInputMode = new Bindable<LaneInputMode>();

        private bool hasReplay => ((SentakkiInputManager)KeyBindingInputManager).ReplayInputHandler != null;

        private SentakkiFramedReplayInputHandler sentakkiFramedReplayInput => (SentakkiFramedReplayInputHandler)((SentakkiInputManager)KeyBindingInputManager).ReplayInputHandler;

        public bool UseSensorMode => hasReplay ? sentakkiFramedReplayInput.UsingSensorMode : laneInputMode.Value == LaneInputMode.Sensor;

        // Default stuff
        protected override Playfield CreatePlayfield() => new SentakkiPlayfield();

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new SentakkiFramedReplayInputHandler(replay);

        protected override ReplayRecorder CreateReplayRecorder(Score score) => new SentakkiReplayRecorder(score, this);

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new SentakkiPlayfieldAdjustmentContainer();

        public override DrawableHitObject<SentakkiHitObject> CreateDrawableRepresentation(SentakkiHitObject h) => null!;

        // protected override ResumeOverlay CreateResumeOverlay() => new SentakkiResumeOverlay();

        protected override PassThroughInputManager CreateInputManager() => new SentakkiInputManager(Ruleset.RulesetInfo);

        /* public override void RequestResume(Action continueResume)
        {
            ResumeOverlay.GameplayCursor = Cursor;
            ResumeOverlay.ResumeAction = continueResume;
            ResumeOverlay.Show();
        } */
    }
}
