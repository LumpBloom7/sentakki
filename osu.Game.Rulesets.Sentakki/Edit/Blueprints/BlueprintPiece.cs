using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints
{
    /// <summary>
    /// A piece of a selection or placement blueprint which visualises an <see cref="SentakkiHitObject"/>.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="SentakkiHitObject"/> which this <see cref="BlueprintPiece{T}"/> visualises.</typeparam>
    public abstract class BlueprintPiece<T> : CompositeDrawable
        where T : SentakkiHitObject
    {
        public BlueprintPiece()
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
        }
        /// <summary>
        /// Updates this <see cref="BlueprintPiece{T}"/> using the properties of a <see cref="SentakkiHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="SentakkiHitObject"/> to reference properties from.</param>
        public virtual void UpdateFrom(T hitObject)
        {
            if (hitObject is SentakkiLanedHitObject lho)
                Position = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, lho.Lane.GetRotationForLane());
            else
                Position = hitObject.Position;
        }
    }
}
