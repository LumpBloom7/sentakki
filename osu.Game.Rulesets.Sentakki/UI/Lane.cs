using System.Linq;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class Lane : Playfield
    {
        public Lane()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.None;
            AddRangeInternal(new Drawable[]{
                HitObjectContainer
            });
        }

        public override void Add(DrawableHitObject hitObject)
        {
            HitObjectContainer.Add(hitObject);
        }
    }
}