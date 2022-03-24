using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.IO;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Sentakki.UI
{
    [Cached]
    public class DrawableSentakkiRuleset : DrawableRuleset<SentakkiHitObject>
    {
        public DrawableSentakkiRuleset(SentakkiRuleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
            foreach (var mod in Mods.OfType<IApplicableToTrack>())
                mod.ApplyToTrack(speedAdjustmentTrack);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            (Config as SentakkiRulesetConfigManager)?.BindWith(SentakkiRulesetSettings.LaneInputMode, laneInputMode);
            (Config as SentakkiRulesetConfigManager)?.BindWith(SentakkiRulesetSettings.GameplayIPC, ipcEnabled);
            TryBroadcastGameplayEvent(new TransmissionData(TransmissionData.InfoType.MetaStartPlay, 0));
        }

        // Input specifics (sensor/button) for replay and gameplay
        private readonly Bindable<LaneInputMode> laneInputMode = new Bindable<LaneInputMode>();

        private bool hasReplay => ((SentakkiInputManager)KeyBindingInputManager).ReplayInputHandler != null;

        private SentakkiFramedReplayInputHandler sentakkiFramedReplayInput => (SentakkiFramedReplayInputHandler)((SentakkiInputManager)KeyBindingInputManager).ReplayInputHandler;

        public bool UseSensorMode => hasReplay ? sentakkiFramedReplayInput.UsingSensorMode : laneInputMode.Value == LaneInputMode.Sensor;

        // Gameplay speed specifics
        private readonly Track speedAdjustmentTrack = new TrackVirtual(0);

        public double GameplaySpeed => speedAdjustmentTrack.Rate;

        /// Network broadcasting stuff
        private readonly GameplayEventBroadcaster eventBroadcaster = new GameplayEventBroadcaster();

        private readonly Bindable<bool> ipcEnabled = new Bindable<bool>();

        /// <summary>
        /// Tries broadcasting an gameplay event.
        /// This depends on user settings, so may not guarantee successful transmission
        /// </summary>
        /// <param name="packet">The packet to be transmitted</param>
        public void TryBroadcastGameplayEvent(TransmissionData packet)
        {
            if (ipcEnabled.Value)
                eventBroadcaster.Broadcast(packet);
        }

        // Default stuff
        protected override Playfield CreatePlayfield() => new SentakkiPlayfield();

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new SentakkiFramedReplayInputHandler(replay);

        protected override ReplayRecorder CreateReplayRecorder(Score score) => new SentakkiReplayRecorder(score, this);

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new SentakkiPlayfieldAdjustmentContainer();

        public override DrawableHitObject<SentakkiHitObject> CreateDrawableRepresentation(SentakkiHitObject h) => null;

        protected override ResumeOverlay CreateResumeOverlay() => new SentakkiResumeOverlay();

        protected override Framework.Input.PassThroughInputManager CreateInputManager() => new SentakkiInputManager(Ruleset?.RulesetInfo);

        protected override void Dispose(bool isDisposing)
        {
            eventBroadcaster.Dispose();
            base.Dispose(isDisposing);
        }
    }
}
