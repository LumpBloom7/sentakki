using osu.Framework.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints
{
    public partial class SentakkiPlacementBlueprint<T> : PlacementBlueprint where T : HitObject, new()
    {
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public new T HitObject => (T)base.HitObject;

        public SentakkiPlacementBlueprint()
            : base(new T())
        {
            Anchor = Origin = Anchor.Centre;
        }
    }
}
