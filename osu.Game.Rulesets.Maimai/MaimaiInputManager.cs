// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.UI;
using System.Collections.Generic;
using System.ComponentModel;

namespace osu.Game.Rulesets.Maimai
{
    public class MaimaiInputManager : RulesetInputManager<MaimaiAction>
    {
        public IEnumerable<MaimaiAction> PressedActions => KeyBindingContainer.PressedActions;

        public MaimaiInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }
    }

    public enum MaimaiAction
    {
        [Description("Button 1")]
        Button1,

        [Description("Button 2")]
        Button2,
    }
}
