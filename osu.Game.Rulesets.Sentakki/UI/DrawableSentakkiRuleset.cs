using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Sentakki.UI
{
    [Cached]
    public class DrawableSentakkiRuleset : DrawableRuleset<SentakkiHitObject>
    {
        public DrawableSentakkiRuleset(SentakkiRuleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
        }

        private readonly Track speedAdjustmentTrack = new TrackVirtual(0);

        public double GameplaySpeed => speedAdjustmentTrack.Rate;

        [BackgroundDependencyLoader(true)]
        private void load()
        {
            foreach (var mod in Mods.OfType<IApplicableToTrack>())
                mod.ApplyToTrack(speedAdjustmentTrack);
        }

        protected override Playfield CreatePlayfield() => new SentakkiPlayfield();

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new SentakkiFramedReplayInputHandler(replay);

        protected override ReplayRecorder CreateReplayRecorder(Replay replay) => new SentakkiReplayRecorder(replay);

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new SentakkiPlayfieldAdjustmentContainer();

        public override DrawableHitObject<SentakkiHitObject> CreateDrawableRepresentation(SentakkiHitObject h) => null;

        protected override ResumeOverlay CreateResumeOverlay() => new SentakkiResumeOverlay();

        protected override Framework.Input.PassThroughInputManager CreateInputManager() => new SentakkiInputManager(Ruleset?.RulesetInfo);
    }
}
