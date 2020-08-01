using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Audio.Track;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using System.Collections.Generic;
using System;

namespace osu.Game.Rulesets.Sentakki.UI
{
    [Cached]
    public class DrawableSentakkiRuleset : DrawableRuleset<SentakkiHitObject>
    {
        public DrawableSentakkiRuleset(SentakkiRuleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
        }

        private Track speedAdjustmentTrack => workingBeatmap.Value.Track;

        public double GameplaySpeed => speedAdjustmentTrack.Rate;

        private Bindable<WorkingBeatmap> workingBeatmap;

        [BackgroundDependencyLoader(true)]
        private void load(Bindable<WorkingBeatmap> WorkingBeatmap)
        {
            workingBeatmap = WorkingBeatmap;
        }

        protected override Playfield CreatePlayfield() => new SentakkiPlayfield();

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new SentakkiFramedReplayInputHandler(replay);

        protected override ReplayRecorder CreateReplayRecorder(Replay replay) => new SentakkiReplayRecorder(replay);

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new SentakkiPlayfieldAdjustmentContainer();

        public override DrawableHitObject<SentakkiHitObject> CreateDrawableRepresentation(SentakkiHitObject h)
        {
            switch (h)
            {
                case Slide slide:
                    return new DrawableSlide(slide);

                case Touch touchNote:
                    return new DrawableTouch(touchNote);

                case Hold holdNote:
                    return new DrawableHold(holdNote);

                case TouchHold touchHold:
                    return new DrawableTouchHold(touchHold);

                case Tap tapNote:
                    return new DrawableTap(tapNote);
            }

            return null;
        }

        protected override ResumeOverlay CreateResumeOverlay() => new SentakkiResumeOverlay();

        protected override Framework.Input.PassThroughInputManager CreateInputManager() => new SentakkiInputManager(Ruleset?.RulesetInfo);
    }
}
