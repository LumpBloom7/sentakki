using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Input;
using osu.Game.Beatmaps;
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
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Sentakki.UI
{
    [Cached]
    public partial class DrawableSentakkiRuleset : DrawableRuleset<SentakkiHitObject>
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
            (Config as SentakkiRulesetConfigManager)?.BindWith(SentakkiRulesetSettings.LaneInputMode, laneInputMode);

            FrameStableComponents.Add(slideFanChevronsTextures);
        }

        // Input specifics (sensor/button) for replay and gameplay
        private readonly Bindable<LaneInputMode> laneInputMode = new Bindable<LaneInputMode>();

        private bool hasReplay => ((SentakkiInputManager)KeyBindingInputManager).ReplayInputHandler != null;

        private SentakkiFramedReplayInputHandler sentakkiFramedReplayInput => (SentakkiFramedReplayInputHandler)((SentakkiInputManager)KeyBindingInputManager).ReplayInputHandler;

        public bool UseSensorMode => hasReplay ? sentakkiFramedReplayInput.UsingSensorMode : laneInputMode.Value == LaneInputMode.Sensor;

        // Gameplay speed specifics
        private readonly Track speedAdjustmentTrack = new TrackVirtual(0);

        public double GameplaySpeed => speedAdjustmentTrack.Rate;

        // Default stuff
        protected override Playfield CreatePlayfield() => new SentakkiPlayfield();

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new SentakkiFramedReplayInputHandler(replay);

        protected override ReplayRecorder CreateReplayRecorder(Score score) => new SentakkiReplayRecorder(score, this);

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new SentakkiPlayfieldAdjustmentContainer();

        public override DrawableHitObject<SentakkiHitObject> CreateDrawableRepresentation(SentakkiHitObject h) => null!;

        protected override ResumeOverlay CreateResumeOverlay() => new SentakkiResumeOverlay();

        protected override PassThroughInputManager CreateInputManager() => new SentakkiInputManager(Ruleset.RulesetInfo);
    }
}
